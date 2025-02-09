
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Interfaces;
using Genrpg.RequestServer.Services.Clients;
using Genrpg.RequestServer.Services.Login;
using Genrpg.RequestServer.Rewards.Services;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Services.NoUsers;
using Genrpg.RequestServer.Services.Admin;
using Genrpg.RequestServer.BoardGame.Services;
using Genrpg.RequestServer.PlayerData.Services;
using Genrpg.RequestServer.Activities.Services;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.RequestServer.UserMail.Services;
using Genrpg.RequestServer.Spawns.Services;
using Genrpg.RequestServer.Purchasing.Services;
using Genrpg.RequestServer.BoardGame.BoardGen;

namespace Genrpg.RequestServer.Setup
{
    public class WebsiteSetupService : BaseServerSetupService
    {
        public WebsiteSetupService(IServiceLocator loc) : base(loc) { }

        protected override void AddServices()
        {
            base.AddServices();
            Set<IAuthWebService>(new AuthWebService());
            Set<IClientWebService>(new ClientWebService());
            Set<IAdminService>(new LoginAdminService());
            Set<INoUserWebService>(new NoUserWebService());
            Set<IWebServerService>(new WebServerService());
            Set<IWebRewardService>(new WebRewardService());
            Set<ILoginPlayerDataService>(new LoginPlayerDataService());
            Set<IServerPurchasingService>(new ServerPurchasingService());


            // Board game
            Set<IBoardService>(new BoardService()); 
            Set<IBoardGenService>(new BoardGenService());   
            Set<IServerActivityService>(new ServerActivityService());
            Set<IDailyResetService>(new DailyResetService());
            Set<IUserMailService>(new UserMailService());
            Set<IDiceRollService>(new DiceRollService());
            Set<IHourlyUpdateService>(new HourlyUpdateService());
            Set<IBoardModeService>(new BoardModeService());
            Set<IBoardPrizeService>(new BoardPrizeService());   
            Set<IWebSpawnService>(new WebSpawnService());   

            _loc.ResolveSelf();
            _loc.Resolve(this);
        }

        public override async Task FinalSetup()
        {
            await base.FinalSetup();
        }
    }
}
