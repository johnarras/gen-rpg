using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Core
{
    public abstract class BasePubSubHelper<M, H> : IPubSubHelper where M : class, IPubSubMessage where H : IPubSubMessageHandler
    {
        public abstract string BaseTopicName();

        protected ServiceBusClient _serviceBusClient = null;
        protected ServiceBusAdministrationClient _adminClient = null;
        protected string _topicName = null;
        protected ServiceBusSender _sender = null;
        protected ServiceBusReceiver _receiver = null;
        protected string _subscriptionName = null;
        protected CancellationToken _token = CancellationToken.None;


        protected ILogService _logService = null;

        protected SetupDictionaryContainer<Type, H> _handlers = new();

        public bool IsValidMessage(IPubSubMessage message)
        {
            if (message is M m)
            {
                return true;
            }
            return false;
        }

        public async Task Init(ServiceBusClient client, ServiceBusAdministrationClient adminClient, string serverId, string env,  CancellationToken token)
        {
            _serviceBusClient = client;
            _adminClient = adminClient;
            _token = token;
            _topicName = BaseTopicName() + "." + env;
            _subscriptionName = serverId + "." + env;

            Response<bool> response = await _adminClient.TopicExistsAsync(_topicName, token);

            if (!response.Value)
            {
                CreateTopicOptions options = new CreateTopicOptions(_topicName)
                {
                    AutoDeleteOnIdle = CloudCommsConstants.EndpointDeleteTime,
                    DefaultMessageTimeToLive = CloudCommsConstants.MessageDeleteTime,
                };

                await _adminClient.CreateTopicAsync(options);
            }

            _sender = _serviceBusClient.CreateSender(_topicName);

            if (serverId == CloudServerNames.Editor.ToLower() ||
                serverId.IndexOf("minst") >= 0)
            {
                return;
            }

            _ = Task.Run(() => RunReceiver(token));
        }

        public void SendMessage(IPubSubMessage message)
        {
            if (message is M m)
            {
                PubSubMessageEnvelope envelope = new PubSubMessageEnvelope() { Message = m };

                ServiceBusMessage serviceBusMessage = new ServiceBusMessage(SerializationUtils.Serialize(envelope));
                _ = Task.Run(() => _sender.SendMessageAsync(serviceBusMessage));
            }
            else
            {
                _logService.Error("Sent incorrect message of type " + message.GetType().Name + " to topic " + _topicName);
            }
        }

        protected async Task RunReceiver(CancellationToken token)
        {           
            try
            {
                Response<bool> response = await _adminClient.SubscriptionExistsAsync(_topicName, _subscriptionName, token);

                if (!response.Value)
                {
                    CreateSubscriptionOptions createOptions = new CreateSubscriptionOptions(_topicName, _subscriptionName)
                    {
                        AutoDeleteOnIdle = CloudCommsConstants.EndpointDeleteTime,
                        DefaultMessageTimeToLive = CloudCommsConstants.MessageDeleteTime,
                    };

                    await _adminClient.CreateSubscriptionAsync(createOptions, token);
                }

                while (_handlers == null)
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                }

                ServiceBusReceiverOptions receiverOptions = new ServiceBusReceiverOptions()
                {
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                    PrefetchCount = 50,                   
                };
                _receiver = _serviceBusClient.CreateReceiver(_topicName, _subscriptionName, receiverOptions);
                _logService.Message("PubSubReceiver: " + _topicName + ":" + _subscriptionName);

                while (true)
                {
                    IReadOnlyList<ServiceBusReceivedMessage> messages = await _receiver.ReceiveMessagesAsync(50, TimeSpan.FromSeconds(1.0f), token);

                    foreach (ServiceBusReceivedMessage message in messages)
                    {
                        PubSubMessageEnvelope envelope = SerializationUtils.Deserialize<PubSubMessageEnvelope>(Encoding.UTF8.GetString(message.Body));

                        if (_handlers == null)
                        {
                            throw new Exception("Cloud PubSub handlers not set up");
                        }

                        if (_handlers.TryGetValue(envelope.Message.GetType(), out H handler))
                        {
                            await handler.HandleMessage(envelope.Message, _token);
                        }
                    }
                }
            }
            catch (OperationCanceledException ce)
            {
                _logService.Info("Shut down PubSub listener " + ce.Message + " " + _topicName + ":" + _subscriptionName);
            }
            catch (Exception e)
            {
                _logService.Exception(e, "PubSubReceiver " + e.Message + " " + _topicName + ":" + _subscriptionName);
            }
            finally
            {
                await _receiver.DisposeAsync();
            }
        }

    }
}
