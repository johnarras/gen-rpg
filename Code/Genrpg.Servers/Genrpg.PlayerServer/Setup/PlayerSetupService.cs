using Genrpg.PlayerServer.Admin;
using Genrpg.PlayerServer.Managers;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.Setup
{
    public class PlayerSetupService : BaseServerSetupService
    {

        public PlayerSetupService(IServiceLocator loc) : base(loc) { }  

        protected override void AddServices()
        {
            base.AddServices();
            Set<IPlayerService>(new PlayerService());
            Set<IAdminService>(new PlayerAdminService());
        }
    }
}
