using Genrpg.PlayerServer.MessageHandlers;
using Genrpg.PlayerServer.RequestHandlers;
using Genrpg.PlayerServer.Setup;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.MainServer;
using Genrpg.Shared.Setup.Services;

namespace Genrpg.PlayerServer
{
    public class PlayerServerMain : BaseServer
    {

        protected override string GetServerId(object data)
        {
            return CloudServerNames.Player;
        }

        protected override SetupService GetSetupService(object data)
        {
            return new PlayerSetupService();
        }

        protected override void SetupCustomCloudMessagingHandlers()
        {
            _cloudCommsService.SetQueueMessageHandlers(_reflectionService.SetupDictionary<Type, IPlayerMessageHandler>(_gs));
            _cloudCommsService.SetRequestHandlers(_reflectionService.SetupDictionary<Type, IPlayerRequestHandler>(_gs));
        }
    }
}