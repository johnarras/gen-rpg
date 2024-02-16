using Genrpg.MonsterServer.MessageHandlers;
using Genrpg.MonsterServer.Setup;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.MainServer;
using Genrpg.Shared.Setup.Services;

namespace Genrpg.MonsterServer
{
    public class MonsterServerMain : BaseServer<ServerGameState, MonsterSetupService, IMonsterMessageHandler>
    {
        protected override string GetServerId(object data)
        {
            return CloudServerNames.Monster;
        }
    }
}