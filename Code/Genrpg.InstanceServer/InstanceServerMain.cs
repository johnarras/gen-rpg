using Genrpg.InstanceServer.MessageHandlers;
using Genrpg.InstanceServer.RequestHandlers;
using Genrpg.InstanceServer.Setup;
using Genrpg.ServerShared.CloudMessaging.Constants;
using Genrpg.ServerShared.MainServer;
using Genrpg.Shared.Setup.Services;

namespace Genrpg.InstanceServer
{
    public class InstanceServerMain : BaseServer
    {
        protected override string GetServerId(object data)
        {
            return CloudServerNames.Instances;
        }

        protected override SetupService GetSetupService(object data)
        {
            return new InstanceSetupService();
        }

        protected override void SetupMessagingHandlers()
        {
            _cloudMessageService.SetMessageHandlers(_reflectionService.SetupDictionary<Type,IInstanceMessageHandler>(_gs));
            _cloudMessageService.SetRequestHandlers(_reflectionService.SetupDictionary<Type,IInstanceRequestHandler>(_gs));
        }
    }
}