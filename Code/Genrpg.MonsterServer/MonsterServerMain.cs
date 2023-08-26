using Genrpg.MonsterServer.MessageHandlers;
using Genrpg.MonsterServer.Setup;
using Genrpg.ServerShared.CloudMessaging.Constants;
using Genrpg.ServerShared.MainServer;
using Genrpg.Shared.Setup.Services;

namespace Genrpg.MonsterServer
{
    public class MonsterServerMain : BaseServer
    {

        protected override string GetServerId(object data)
        {
            return CloudServerNames.Monster;
        }

        protected override SetupService GetSetupService(object data)
        {
            return new MonsterSetupService();
        }

        protected override void SetupMessageHandlers()
        {
            _cloudMessageService.SetMessageHandlers(_reflectionService.SetupDictionary<Type, IMonsterMessageHandler>(_gs));
        }
    }
}