using Genrpg.InstanceServer.Managers;
using Genrpg.InstanceServer.Admin;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;

namespace Genrpg.InstanceServer.Setup
{
    public class InstanceLocatorSetup : BaseServerLocatorSetup
    {
        public override void Setup(IGameState gs)
        {
            base.Setup(gs);

            gs.loc.Set<IInstanceManagerService>(new InstanceManagerService());
            gs.loc.Set<IAdminService>(new InstanceAdminService());
        }
    }
}
