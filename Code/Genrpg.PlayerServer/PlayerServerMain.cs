using Genrpg.PlayerServer.MessageHandlers;
using Genrpg.PlayerServer.Setup;
using Genrpg.ServerShared.CloudMessaging.Constants;
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

        protected override void SetupMessageHandlers()
        {
            _cloudMessageService.SetMessageHandlers(_reflectionService.SetupDictionary<Type, IPlayerMessageHandler>(_gs));
        }
    }
}