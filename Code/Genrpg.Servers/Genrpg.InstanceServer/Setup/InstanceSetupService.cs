using Genrpg.InstanceServer.Admin;
using Genrpg.InstanceServer.Managers;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.Setup
{
    public class InstanceSetupService : BaseServerSetupService
    {

        public InstanceSetupService(IServiceLocator loc) : base(loc) { }

        protected override void AddServices()
        {
            base.AddServices();
            Set<IInstanceManagerService>(new InstanceManagerService());
            Set<IAdminService>(new InstanceAdminService());
        }
    }
}
