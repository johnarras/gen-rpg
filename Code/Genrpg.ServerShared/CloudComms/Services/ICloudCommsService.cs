using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Services
{
    public interface ICloudCommsService : IInitializable, IDisposable
    {
        string GetFullServerName(string serverId);

        void SetQueueMessageHandlers<H>(Dictionary<Type, H> handlers) where H : IQueueMessageHandler;
        void SendQueueMessage(string serverId, IQueueMessage cloudMessage);
        void SendQueueMessages(string serverId, List<IQueueMessage> cloudMessages);
        Task<TResponse> SendResponseMessageAsync<TResponse>(string serverId, IRequestQueueMessage requestMessage) where TResponse : IResponseQueueMessage;
        void SendResponseMessageWithHandler<TResponse>(string serverId, IRequestQueueMessage requestMessage,
            Action<TResponse> responseHandler) where TResponse : IResponseQueueMessage;



        void SendPubSubMessage(ServerGameState gs, IPubSubMessage message);
        void SetupPubSubMessageHandlers(ServerGameState gs);
    }
}
