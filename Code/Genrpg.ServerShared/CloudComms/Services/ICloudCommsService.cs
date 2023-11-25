using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Requests.Entities;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Services
{
    public interface ICloudCommsService : ISetupService, IDisposable
    {
        string GetFullServerName(string serverId);
        string GetFullServerNameForMapInstance(string mapId, string mapInstanceId);
        void SetQueueMessageHandlers<H>(Dictionary<Type, H> handlers) where H : IQueueMessageHandler;
        void SendQueueMessage(string serverId, IQueueMessage cloudMessage);
        void SendQueueMessages(string serverId, List<IQueueMessage> cloudMessages);

        void SendRequest(string serverId, IRequest request, Action<ResponseEnvelope> responseAction);
        Task<ResponseEnvelope> SendRequestAsync(string serverId, IRequest request);
        void SetRequestHandlers<H>(Dictionary<Type, H> handlers) where H : IRequestHandler;


        void SendPubSubMessage(ServerGameState gs, IPubSubMessage message);
        void SetupPubSubMessageHandlers(ServerGameState gs);
    }
}
