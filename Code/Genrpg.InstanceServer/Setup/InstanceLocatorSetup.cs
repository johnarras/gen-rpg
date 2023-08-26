using Genrpg.InstanceServer.Services;
using Genrpg.ServerShared.CloudMessaging.Services;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
