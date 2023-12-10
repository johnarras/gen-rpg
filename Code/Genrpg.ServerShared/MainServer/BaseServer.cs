using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Setup.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.MainServer
{
    public interface IBaseServer
    {

    }


    public abstract class BaseServer<TGameState,TSetupService,IQMessageHandler> : IBaseServer 
        where TGameState: ServerGameState, new()
        where TSetupService: SetupService, new()
        where IQMessageHandler : IQueueMessageHandler
    {
        protected TGameState _gs;
        protected CancellationTokenSource _tokenSource = new CancellationTokenSource();
        protected string _serverId;
        protected ICloudCommsService _cloudCommsService;
        protected IReflectionService _reflectionService;

        public virtual async Task Init(object data, CancellationToken serverToken)
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(serverToken, _tokenSource.Token);
            _serverId = GetServerId(data);

            _gs = await SetupUtils.SetupFromConfig<TGameState>(this, _serverId, new TSetupService(),
                _tokenSource.Token);

            _cloudCommsService.SetQueueMessageHandlers(_reflectionService.SetupDictionary<Type, IQMessageHandler>(_gs));
        
            _cloudCommsService.SetupPubSubMessageHandlers(_gs);

            _cloudCommsService.SendPubSubMessage(_gs, new ServerStartedAdminMessage() { ServerId = _serverId });

            await Task.CompletedTask;
        }

        protected abstract string GetServerId(object data);
    }
}
