using Genrpg.PlayerServer.MessageHandlers;
using Genrpg.PlayerServer.Setup;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.MainServer;
using Genrpg.Shared.Setup.Services;

namespace Genrpg.PlayerServer
{
    public class PlayerServerMain : BaseServer<ServerGameState,PlayerSetupService,IPlayerMessageHandler>
    {
        protected override string GetServerId(object data)
        {
            return CloudServerNames.Player;
        }
    }
}