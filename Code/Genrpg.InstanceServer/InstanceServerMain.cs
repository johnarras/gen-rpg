using Genrpg.InstanceServer.MessageHandlers;
using Genrpg.InstanceServer.Setup;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.MainServer;
using Genrpg.Shared.Setup.Services;

namespace Genrpg.InstanceServer
{
    public class InstanceServerMain : BaseServer<ServerGameState,InstanceSetupService,IInstanceMessageHandler>
    {
        protected override string GetServerId(object data)
        {
            return CloudServerNames.Instance.ToString();
        }
    }
}