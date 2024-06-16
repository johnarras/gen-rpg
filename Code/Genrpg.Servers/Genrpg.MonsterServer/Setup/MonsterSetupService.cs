using Genrpg.MonsterServer.Admin;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Setup.Services;

namespace Genrpg.MonsterServer.Setup
{
    public class MonsterSetupService : BaseServerSetupService
    {
        public MonsterSetupService(IServiceLocator loc) : base(loc) { }


        protected override void AddServices()
        {
            base.AddServices();
            Set<IAdminService>(new MonsterAdminService());
        }
    }
}
