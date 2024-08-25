using Genrpg.MapServer.Admin.Services;
using Genrpg.MapServer.CharMail.Services;
using Genrpg.MapServer.Maps.Services;
using Genrpg.MapServer.Rewards.Services;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Setup.MapServer
{
    public class MapServerSetupService : BaseServerSetupService
    {
        public MapServerSetupService(IServiceLocator loc) : base(loc) { } 

        protected override void AddServices()
        {
            base.AddServices();
            Set<IAdminService>(new MapServerAdminService());
            Set<IMapServerService>(new MapServerService());
            Set<IRewardService>(new ServerRewardService());
            Set<ICharMailService>(new CharMailService());
        }
    }
}
