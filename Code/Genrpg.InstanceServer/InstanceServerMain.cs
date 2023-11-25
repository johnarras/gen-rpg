using Genrpg.InstanceServer.MessageHandlers;
using Genrpg.InstanceServer.RequestHandlers;
using Genrpg.InstanceServer.Setup;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.MainServer;
using Genrpg.Shared.Setup.Services;

namespace Genrpg.InstanceServer
{
    public class InstanceServerMain : BaseServer
    {
        protected override string GetServerId(object data)
        {
            return CloudServerNames.Instance.ToString();
        }

        protected override SetupService GetSetupService(object data)
        {
            return new InstanceSetupService();
        }

        protected override void SetupCustomCloudMessagingHandlers()
        {
            _cloudCommsService.SetQueueMessageHandlers(_reflectionService.SetupDictionary<Type,IInstanceMessageHandler>(_gs));
            _cloudCommsService.SetRequestHandlers(_reflectionService.SetupDictionary<Type,IInstanceRequestHandler>(_gs));
        }
    }
}