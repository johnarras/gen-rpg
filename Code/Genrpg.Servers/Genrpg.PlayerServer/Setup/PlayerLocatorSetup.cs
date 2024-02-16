using Genrpg.PlayerServer.Managers;
using Genrpg.PlayerServer.Admin;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.Setup
{
    public class PlayerLocatorSetup : BaseServerLocatorSetup
    {
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
            gs.loc.Set<IPlayerService>(new PlayerService());
            gs.loc.Set<IAdminService>(new PlayerAdminService());
        }
    }
}
