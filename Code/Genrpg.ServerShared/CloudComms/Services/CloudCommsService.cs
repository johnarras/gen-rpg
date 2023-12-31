﻿using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Genrpg.ServerShared.Core;
using Azure.Messaging.ServiceBus.Administration;
using Genrpg.ServerShared.DataStores.Constants;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.Shared.Reflection.Services;
using Genrpg.ServerShared.CloudComms.PubSub.Managers;
using Genrpg.ServerShared.CloudComms.Queues.Managers;
using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;
using MongoDB.Driver.Core.WireProtocol.Messages;

namespace Genrpg.ServerShared.CloudComms.Services
{

    public class CloudCommsService : ICloudCommsService
    {
        private IReflectionService _reflectionService = null;


        private ServerGameState _serverGameState = null;
        private string _env;
        private string _serverId;
        private CancellationToken _token = CancellationToken.None;

        // Core ServiceBus
        private ServiceBusClient _serviceBusClient = null;
        private ServiceBusAdministrationClient _adminClient = null;

        // Queues
        private CloudQueueManager _queueManager = null;

        // Pubsub
        private CloudPubSubManager _pubSubManager = null;

        public async Task Setup(GameState gs, CancellationToken token)
        {
            _serverGameState = gs as ServerGameState;
            _token = token;
            _env = _serverGameState.config.MessagingEnv.ToLower();
            _serverId = _serverGameState.config.ServerId.ToLower();
            string serviceBusConnectionString = _serverGameState.config.GetConnectionString(ConnectionNames.ServiceBus);
            _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            _adminClient = new ServiceBusAdministrationClient(serviceBusConnectionString);

            _queueManager = new CloudQueueManager();
            await _queueManager.Init(_serverGameState, _serviceBusClient, _adminClient, _serverId, _env, token);

            _pubSubManager = new CloudPubSubManager();
            await _pubSubManager.Init(_serverGameState, _serviceBusClient, _adminClient, _reflectionService, _serverId, _env, token);

        }

        public void Dispose()
        {
            _serviceBusClient?.DisposeAsync();
        }

        #region queue

        public string GetFullServerName(string serverId)
        {
            return _queueManager.GetFullQueueName(serverId);
        }

        public void SetQueueMessageHandlers<H>(Dictionary<Type, H> handlers) where H : IQueueMessageHandler
        {
            _queueManager.SetQueueMessageHandlers(handlers);
        }

        public void SendQueueMessage(string serverId, IQueueMessage cloudMessage)
        {
            _queueManager.SendQueueMessages(serverId, new List<IQueueMessage>() { cloudMessage });
        }

        public void SendQueueMessages(string serverId, List<IQueueMessage> cloudMessages)
        {
            _queueManager.SendQueueMessages(serverId, cloudMessages);
        }

        public async Task<TResponse> SendResponseMessageAsync<TResponse>(string serverId, IRequestQueueMessage requestMessage) where TResponse : IResponseQueueMessage
        {
            return await _queueManager.SendRequestResponseQueueMessage<TResponse>(serverId, requestMessage);
        }


        public void SendResponseMessageWithHandler<TResponse>(string serverId, IRequestQueueMessage requestMessage, Action<TResponse> responseHandler) where TResponse : IResponseQueueMessage
        {
            Task.Run(()=>SendAsyncRequestWithHandler(serverId, requestMessage, responseHandler));
        }

        private async Task SendAsyncRequestWithHandler<TResponse>(string serverId, IRequestQueueMessage requestMessage, Action<TResponse> responseHandler) where TResponse : IResponseQueueMessage
        {
            TResponse response = await SendResponseMessageAsync<TResponse>(serverId, requestMessage);
            responseHandler?.Invoke(response);
        }
        #endregion

            #region pubsub

        public void SendPubSubMessage(ServerGameState gs, IPubSubMessage message)
        {
            _pubSubManager.SendMessage(gs, message);
        }

        public void SetupPubSubMessageHandlers(ServerGameState gs)
        {
            _pubSubManager.SetupPubSubMessageHandlers(gs, _reflectionService);
        }

        #endregion

    }
}
