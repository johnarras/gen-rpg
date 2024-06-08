using Genrpg.MonsterServer.Admin;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MonsterServer.Setup
{
    public class MonsterLocatorSetup : BaseServerLocatorSetup
    {
        public override void Setup(IGameState gs)
        {
            base.Setup(gs);

            gs.loc.Set<IAdminService>(new MonsterAdminService());
        }
    }
}
