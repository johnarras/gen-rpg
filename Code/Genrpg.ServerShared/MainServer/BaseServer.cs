using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Setup.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.MainServer
{
    public abstract class BaseServer
    {
        protected ServerGameState _gs;
        protected CancellationTokenSource _tokenSource = new CancellationTokenSource();
        protected string _serverId;
        protected ICloudCommsService _cloudCommsService;
        protected IReflectionService _reflectionService;

        public virtual async Task Init(object data, CancellationToken serverToken)
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(serverToken, _tokenSource.Token);
            _serverId = GetServerId(data);
            SetupService setupService = GetSetupService(data);

            _gs = await SetupUtils.SetupFromConfig<ServerGameState>(this, _serverId, setupService,
                _tokenSource.Token);

            SetupCustomCloudMessagingHandlers();
            _cloudCommsService.SetupPubSubMessageHandlers(_gs);

            _cloudCommsService.SendPubSubMessage(_gs, new ServerStartedAdminMessage() { ServerId = _serverId });

            await Task.CompletedTask;
        }

        public virtual void UpdateFromNewGameData(GameData gameData)
        {
            _gs.data = gameData;
            _gs.logger.Message("Update GameData on " + GetType().Name);
        }

        protected abstract string GetServerId(object data);
        protected abstract SetupService GetSetupService(object data);
        protected abstract void SetupCustomCloudMessagingHandlers();
    }
}
