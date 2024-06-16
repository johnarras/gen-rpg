using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Setup.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Genrpg.ServerShared.Config;
using Genrpg.Shared.HelperClasses;

namespace Genrpg.ServerShared.MainServer
{
    public interface IBaseServer
    {
        CancellationToken GetToken();
        ServerGameState GetServerGameState();
    }


    public abstract class BaseServer<TGameState,TSetupService,IQMessageHandler> : IBaseServer 
        where TGameState: ServerGameState
        where TSetupService: SetupService
        where IQMessageHandler : IQueueMessageHandler
    {
        protected TGameState _context;
        protected CancellationTokenSource _tokenSource = new CancellationTokenSource();
        protected string _serverId;
        protected ICloudCommsService _cloudCommsService;
        protected IServerConfig _config = null;

        public virtual CancellationToken GetToken ()
        { 
            return _tokenSource?.Token ?? CancellationToken.None;
        }

        public virtual ServerGameState GetServerGameState()
        {
            return _context;
        }

        protected virtual async Task PreInit(object data, object parent, CancellationToken serverToken)
        {
            await Task.CompletedTask;
        }


        protected virtual async Task FinalInit(object data, object parentObject, CancellationToken serverToken)
        {
            await Task.CompletedTask;
        }


        private SetupDictionaryContainer<Type, IQMessageHandler> _queueHandlers = new();

        public async Task Init(object data, object parentObject, CancellationToken serverToken)
        {

            await PreInit(data, parentObject, serverToken);

            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(serverToken, _tokenSource.Token);
            _serverId = GetServerId(data);

            _context = await SetupUtils.SetupFromConfig<TGameState,TSetupService>(this, _serverId, 
                _tokenSource.Token);

            _cloudCommsService.SetQueueMessageHandlers(_queueHandlers.GetDict());
        
            _cloudCommsService.SendPubSubMessage(new ServerStartedAdminMessage() { ServerId = _serverId });

            _context.loc.Resolve(parentObject);
            _context.loc.Resolve(this);
            await FinalInit(data, parentObject, serverToken);

            await Task.CompletedTask;
        }

        protected abstract string GetServerId(object data);
    }
}
