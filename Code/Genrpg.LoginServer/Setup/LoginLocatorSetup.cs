using Genrpg.LoginServer.Services.Admin;
using Genrpg.LoginServer.Services.Clients;
using Genrpg.LoginServer.Services.Login;
using Genrpg.LoginServer.Services.NoUsers;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Core.Entities;

namespace Genrpg.LoginServer.Setup
{
    public class LoginLocatorSetup : BaseServerLocatorSetup
    {
        public override void Setup(IGameState gs)
        {
            gs.loc.Set<ILoginService>(new LoginService());
            gs.loc.Set<IClientService>(new ClientService());
            gs.loc.Set<IAdminService>(new LoginAdminService());
            gs.loc.Set<INoUserService>(new NoUserService());

            base.Setup(gs);
        }
    }
}
