using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.Services.Admin;
using Genrpg.LoginServer.Services.Clients;
using Genrpg.LoginServer.Services.Login;
using Genrpg.LoginServer.Services.LoginServer;
using Genrpg.LoginServer.Services.NoUsers;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Utils;
using System;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Setup
{
    public class LoginSetupService : BaseServerSetupService
    {
        public LoginSetupService(IServiceLocator loc) : base(loc) { }   

        private ICloudCommsService _cloudCommsService = null;

        protected override void AddServices()
        {
            base.AddServices();
            Set<IAuthWebService>(new AuthWebService());
            Set<IClientWebService>(new ClientWebService());
            Set<IAdminService>(new LoginAdminService());
            Set<INoUserWebService>(new NoUserWebService());
            Set<IWebServerService>(new WebServerService());
            _loc.ResolveSelf();
            _loc.Resolve(this);
        }

        public override async Task FinalSetup()
        { 
            await base.FinalSetup();
        }
    }
}
