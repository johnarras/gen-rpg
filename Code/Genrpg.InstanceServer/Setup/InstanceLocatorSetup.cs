using Genrpg.InstanceServer.Services;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;

namespace Genrpg.InstanceServer.Setup
{
    public class InstanceLocatorSetup : BaseServerLocatorSetup
    {
        public override void Setup(GameState gs)
        {
            base.Setup(gs);

            gs.loc.Set<IMapInstanceService>(new MapInstanceService());
        }
    }
}
