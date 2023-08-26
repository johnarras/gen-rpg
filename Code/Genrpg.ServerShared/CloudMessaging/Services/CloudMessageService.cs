using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Utils;
using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus.Administration;
using System.Runtime.InteropServices;
using Genrpg.ServerShared.CloudMessaging.Messages;
using Genrpg.Shared.Logs.Entities;

namespace Genrpg.ServerShared.CloudMessaging.Services
{

    public interface ICloudMessageService : ISetupService, IDisposable
    {
        string GetFullQueueName(string serverId);
        void SetMessageHandlers<H>(Dictionary<Type, H> handlers) where H : ICloudMessageHandler;
        void SendMessage(string serverId, ICloudMessage cloudMessage);
    }

    public class CloudMessageService : ICloudMessageService
    {
        private string _gamePrefix;
        private string _env;
        private string _serverId;
        private string _myQueueName;
        private bool _didSetup;

        private ServiceBusClient _client = null;
        private ServiceBusSender _sender = null;
        private CancellationToken _token = CancellationToken.None;
        private string _connectionString = null;
        private ServiceBusReceiver _receiver;
        private ServerGameState _sgs = null;
        private Dictionary<Type, ICloudMessageHandler> _handlers;

        public async Task Setup(GameState gs, CancellationToken token)
        {
            _sgs = gs as ServerGameState;
            _token = token;
            _gamePrefix = Game.Prefix;
            _env = _sgs.config.Env;
            _serverId = _sgs.config.ServerId;
            _connectionString = _sgs.config.GetConnectionString("ServiceBus");

            // Not requiring this since it's not necessary for core kill/loot gameplay on one map.
            if (string.IsNullOrEmpty(_connectionString))
            {
                _didSetup = false;
                return;
            }

            _myQueueName = GetFullQueueName(_serverId);

            ServiceBusAdministrationClient adminClient = new ServiceBusAdministrationClient(_connectionString);

            CreateQueueOptions options = new CreateQueueOptions(_myQueueName)
            {
                DefaultMessageTimeToLive = TimeSpan.FromSeconds(20.0f)
            };

            try
            {
                if (!await adminClient.QueueExistsAsync(_myQueueName, token))
                {
                    await adminClient.CreateQueueAsync(options, token);
                }
            }
            catch (Exception e)
            {
                gs.logger.Exception(e, "CloudMessageSetup");
            }

            gs.logger.Message("Created Queue " + _myQueueName);
            _client = new ServiceBusClient(_connectionString);

            _ = Task.Run(() => SetupReceiver(gs.logger, _token));

            await Task.CompletedTask;
        }

        protected string QueueSuffix()
        {
            return ("." + _env).ToLower();
        }

        public string GetFullQueueName(string serverId)
        {
            return (serverId + QueueSuffix()).ToLower();
        }

        public static string GetQueueNameForMap(string mapId, string mapInstanceId)
        {
            return "map" + mapId + "." + mapInstanceId;
        }

        public void SetMessageHandlers<H>(Dictionary<Type,H> handlers) where H : ICloudMessageHandler
        {
            Dictionary<Type, ICloudMessageHandler> newDict = new Dictionary<Type, ICloudMessageHandler>();

            foreach (var handlerType in handlers.Keys)
            {
                newDict[handlerType] = handlers[handlerType];
            }

            _handlers = newDict;
        }

        private async Task SetupReceiver(ILogSystem logger, CancellationToken token)
        {
            ServiceBusReceiverOptions options = new ServiceBusReceiverOptions()
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                PrefetchCount = 50,
            };

            _receiver = _client.CreateReceiver(_myQueueName, options);

            while (_handlers == null)
            {
                await Task.Delay(1, token);
            }

            while (true)
            {
                IReadOnlyList<ServiceBusReceivedMessage> messages = await _receiver.ReceiveMessagesAsync(50, TimeSpan.FromSeconds(1.0f), token);

                foreach (ServiceBusReceivedMessage message in messages)
                {
                    CloudMessageEnvelope envelope = SerializationUtils.Deserialize<CloudMessageEnvelope>(Encoding.UTF8.GetString(message.Body));
                    logger.Message("Received message: " + _myQueueName);

                    if (_handlers == null)
                    {
                        throw new Exception("Cloud message handlers not set up");
                    }

                    if (_handlers.TryGetValue(envelope.Message.GetType(), out ICloudMessageHandler handler))
                    {
                        await handler.HandleMessage(_sgs, envelope.Message, _token);
                    }
                }
            }
        }

        private ConcurrentDictionary<string, ServiceBusSender> _senders = new ConcurrentDictionary<string, ServiceBusSender>();
        public void SendMessage(string serverId, ICloudMessage cloudMessage)
        {
            if (!_didSetup)
            {
                return;
            }

            if (serverId.IndexOf(QueueSuffix()) < 0)
            {
                serverId = GetFullQueueName(serverId);
            }

            CloudMessageEnvelope envelope = new CloudMessageEnvelope()
            {
                ToServerId = serverId,
                FromServerId = _serverId,
                Message = cloudMessage,
            };

            if (!_senders.TryGetValue(envelope.ToServerId, out ServiceBusSender sender))
            {
                sender = _client.CreateSender(envelope.ToServerId);
                _senders[envelope.ToServerId] = sender;
            }

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(SerializationUtils.Serialize(envelope));
            _ = Task.Run(() => sender.SendMessageAsync(serviceBusMessage));

        }


        public void Dispose()
        {
            _client?.DisposeAsync();
        }
    }
}
