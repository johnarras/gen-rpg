﻿using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.DataStores.Constants;
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
using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;
using Genrpg.Shared.Logging.Interfaces;

namespace Genrpg.ServerShared.CloudComms.Queues.Managers
{
    internal class CloudQueueManager
    {
        const double QueueRequestTimeoutSeconds = 5.0f;



        private ServiceBusReceiver _queueReceiver;
        private Dictionary<Type, IQueueMessageHandler> _queueHandlers;
        private string _myQueueName;
        private bool _didSetupQueue = false;

        private ServiceBusClient _serviceBusClient = null;
        private ServiceBusAdministrationClient _adminClient = null;
        private CancellationToken _token;
        private ILogService _logService = null;
        private string _env = null;
        private string _serverId = null;

        public async Task Init(ILogService logService, ServiceBusClient serviceBusClient, ServiceBusAdministrationClient adminClient,
            string serverId, string env, CancellationToken token)
        {
            _serviceBusClient = serviceBusClient;
            _adminClient = adminClient;
            _token = token;
            _env = env;
            _serverId = serverId;
            _logService = logService;

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
                _logService.Exception(e, "CloudMessageSetup");
            }

            _logService.Message("Created Queue " + _myQueueName);

            _ = Task.Run(() => SetupQueueReceiver(_logService, _token));

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

        private async Task SetupQueueReceiver(ILogService logger, CancellationToken token)
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

            try
            {
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
                            if (queueMessage is IResponseQueueMessage responseQueueMessage &&
                                _pendingRequests.TryRemove(responseQueueMessage.RequestId, out PendingQueueRequest pendingRequest))
                            {
                                pendingRequest.Response = responseQueueMessage;
                            }
                            else if (_queueHandlers.TryGetValue(queueMessage.GetType(), out IQueueMessageHandler handler))
                            {
                                await handler.HandleMessage(queueMessage, _token);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException ce)
            {
                _logService.Info("Shutting down cloud listener for " + ce.Message + " " + _serverId);
            }
        }

        private ConcurrentDictionary<string, PendingQueueRequest> _pendingRequests = new ConcurrentDictionary<string, PendingQueueRequest>();
        public async Task<TResponse> SendRequestResponseQueueMessage<TResponse>(string serverId, IRequestQueueMessage requestMessage) where TResponse : IResponseQueueMessage
    {

            PendingQueueRequest pendingQueueRequest = new PendingQueueRequest()
            {
                ToServerId = serverId,
                FromServerId = _serverId,
                SendTime = DateTime.UtcNow,
                Request = requestMessage,
                RequestId = HashUtils.NewGuid().ToString(),
            };
            _pendingRequests[pendingQueueRequest.RequestId] = pendingQueueRequest;
            requestMessage.RequestId = pendingQueueRequest.RequestId;
            requestMessage.FromServerId = _serverId;

            SendQueueMessages(serverId, new List<IQueueMessage>() { requestMessage });

            do
            {
                await Task.Delay(1, _token).ConfigureAwait(false);

                if (pendingQueueRequest.Response != null)
                {
                    return (TResponse) pendingQueueRequest.Response;
                }
            }
            while (pendingQueueRequest.Response == null && 
            (DateTime.UtcNow - pendingQueueRequest.SendTime).TotalSeconds < QueueRequestTimeoutSeconds);

            if (_pendingRequests.TryRemove(pendingQueueRequest.RequestId, out PendingQueueRequest orphanedRequest))
            {
                return (TResponse)orphanedRequest.Response;
            }

            return default;
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
