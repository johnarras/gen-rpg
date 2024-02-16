using Genrpg.MapServer.Services.Maps;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Setup
{
    public class MapServerLocatorSetup : BaseServerLocatorSetup
    {
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
            gs.loc.Set<IAdminService>(new MapServerAdminService());
            gs.loc.Set<IMapServerService>(new MapServerService());  
        }
    }
}
