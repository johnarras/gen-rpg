using Genrpg.MonsterServer.MessageHandlers;
using Genrpg.MonsterServer.Setup;
using Genrpg.PlayerServer.RequestHandlers;
using Genrpg.ServerShared.CloudComms.Constants;
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

        protected override void SetupCustomCloudMessagingHandlers()
        {
            _cloudCommsService.SetQueueMessageHandlers(_reflectionService.SetupDictionary<Type, IMonsterMessageHandler>(_gs));
            _cloudCommsService.SetRequestHandlers(_reflectionService.SetupDictionary<Type, IMonsterRequestHandler>(_gs));
        }
    }
}