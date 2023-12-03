using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.DataStores.Constants;
using Genrpg.Shared.Logs.Entities;
using Genrpg.Shared.Utils;
using MongoDB.Driver.Core.Servers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Constants;

namespace Genrpg.ServerShared.CloudComms.Queues.Managers
{
    internal class CloudQueueManager
    {
        private ServiceBusReceiver _queueReceiver;
        private Dictionary<Type, IQueueMessageHandler> _queueHandlers;
        private string _myQueueName;
        private bool _didSetupQueue = false;

        private ServerGameState _serverGameState = null;
        private ServiceBusClient _serviceBusClient = null;
        private ServiceBusAdministrationClient _adminClient = null;
        private CancellationToken _token;
        private string _env = null;
        private string _serverId = null;

        public async Task Init(ServerGameState gs, ServiceBusClient serviceBusClient, ServiceBusAdministrationClient adminClient,
            string serverId, string env, CancellationToken token)
        {
            _serverGameState = gs;
            _serviceBusClient = serviceBusClient;
            _adminClient = adminClient;
            _token = token;
            _env = env;
            _serverId = serverId;

            _myQueueName = GetFullQueueName(_serverId);

            CreateQueueOptions options = new CreateQueueOptions(_myQueueName)
            {
                AutoDeleteOnIdle = CloudCommsConstants.EndpointDeleteTime,
                DefaultMessageTimeToLive = CloudCommsConstants.MessageDeleteTime,
            };

            try
            {
                if (!await _adminClient.QueueExistsAsync(_myQueueName, token))
                {
                    await _adminClient.CreateQueueAsync(options, token);
                }
            }
            catch (Exception e)
            {
                gs.logger.Exception(e, "CloudMessageSetup");
            }

            gs.logger.Message("Created Queue " + _myQueueName);

            _ = Task.Run(() => SetupQueueReceiver(gs.logger, _token));

            _didSetupQueue = true;
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

        public void SetQueueMessageHandlers<H>(Dictionary<Type, H> handlers) where H : IQueueMessageHandler
        {
            Dictionary<Type, IQueueMessageHandler> newDict = new Dictionary<Type, IQueueMessageHandler>();

            foreach (var handlerType in handlers.Keys)
            {
                newDict[handlerType] = handlers[handlerType];
            }
            _queueHandlers = newDict;
        }

        private async Task SetupQueueReceiver(ILogSystem logger, CancellationToken token)
        {
            ServiceBusReceiverOptions options = new ServiceBusReceiverOptions()
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                PrefetchCount = 50,
            };

            _queueReceiver = _serviceBusClient.CreateReceiver(_myQueueName, options);

            while (_queueHandlers == null)
            {
                await Task.Delay(1, token);
            }

            while (true)
            {
                IReadOnlyList<ServiceBusReceivedMessage> messages = await _queueReceiver.ReceiveMessagesAsync(50, TimeSpan.FromSeconds(1.0f), token);

                foreach (ServiceBusReceivedMessage message in messages)
                {
                    QueueMessageEnvelope envelope = SerializationUtils.Deserialize<QueueMessageEnvelope>(Encoding.UTF8.GetString(message.Body));
                    logger.Message("Received message: " + _myQueueName);

                    if (_queueHandlers == null)
                    {
                        throw new Exception("Cloud message handlers not set up");
                    }

                    foreach (IQueueMessage queueMessage in envelope.Messages)
                    {



                        if (_queueHandlers.TryGetValue(queueMessage.GetType(), out IQueueMessageHandler handler))
                        {
                            await handler.HandleMessage(_serverGameState, queueMessage, _token);
                        }
                    }
                }
            }
        }

        private ConcurrentDictionary<string, ServiceBusSender> _senders = new ConcurrentDictionary<string, ServiceBusSender>();
        public void SendQueueMessages(string serverId, List<IQueueMessage> cloudMessages)
        {
            if (!_didSetupQueue)
            {
                return;
            }

            if (serverId.IndexOf(QueueSuffix()) < 0)
            {
                serverId = GetFullQueueName(serverId);
            }

            QueueMessageEnvelope envelope = new QueueMessageEnvelope()
            {
                ToServerId = serverId,
                FromServerId = _serverId,
                Messages = cloudMessages,
            };

            if (!_senders.TryGetValue(envelope.ToServerId, out ServiceBusSender sender))
            {
                sender = _serviceBusClient.CreateSender(envelope.ToServerId);
                _senders[envelope.ToServerId] = sender;
            }

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(SerializationUtils.Serialize(envelope));
            _ = Task.Run(() => sender.SendMessageAsync(serviceBusMessage));

        }

    }
}
