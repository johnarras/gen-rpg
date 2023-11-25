using Genrpg.Shared.Core.Entities;
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
using Genrpg.ServerShared.CloudComms.Requests.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Constants;
using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.Shared.Reflection.Services;
using Genrpg.ServerShared.CloudComms.PubSub.Managers;
using Genrpg.ServerShared.CloudComms.Queues.Managers;
using Genrpg.ServerShared.CloudComms.Requests.Managers;

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

        // Request
        private CloudRequestManager _requestManager = null;

        public async Task Setup(GameState gs, CancellationToken token)
        {
            _serverGameState = gs as ServerGameState;
            _token = token;
            _env = _serverGameState.config.Env.ToLower();
            _serverId = _serverGameState.config.ServerId.ToLower();
            string serviceBusConnectionString = _serverGameState.config.GetConnectionString(ConnectionNames.ServiceBus);
            _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            _adminClient = new ServiceBusAdministrationClient(serviceBusConnectionString);

            _queueManager = new CloudQueueManager();
            await _queueManager.Init(_serverGameState, _serviceBusClient, _adminClient, _serverId, _env, token);

            _pubSubManager = new CloudPubSubManager();
            await _pubSubManager.Init(_serverGameState, _serviceBusClient, _adminClient, _reflectionService, _serverId, _env, token);

            _requestManager = new CloudRequestManager();
            await _requestManager.Init(_serverGameState, this, _serverId, _env, token);
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

        public string GetFullServerNameForMapInstance(string mapId, string mapInstanceId)
        {
            return "minst" + mapId + "." + mapInstanceId;
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

        #region requests

        public void SendRequest(string serverId, IRequest request, Action<ResponseEnvelope> responseAction)
        {
            _requestManager.SendRequest(serverId, request, responseAction);
        }

        public async Task<ResponseEnvelope> SendRequestAsync(string serverId, IRequest request)
        {
            return await _requestManager.SendRequestAsync(serverId, request);
        }

        public void SetRequestHandlers<H>(Dictionary<Type, H> handlers) where H : IRequestHandler
        {
            _requestManager.SetRequestHandlers(handlers);
        }

        #endregion


    }
}
