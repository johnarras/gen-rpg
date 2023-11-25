using Genrpg.LoginServer.Services.Clients;
using Genrpg.LoginServer.Services.Login;
using Genrpg.LoginServer.Setup;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.GameSettings;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Utils;
using Genrpg.LoginServer.Utils;
using System.ComponentModel;

namespace Genrpg.LoginServer.Core
{
    /// <summary>
    /// This is a minimal amount of webdev used to get us into code that can be used elsewhere easier.
    /// </summary>
    public class WebService 
    {
        private LoginGameState _gs;
        protected IClientService _clientService { get; private set; }
        protected ILoginService _loginService { get; private set; }
        private CancellationTokenSource _serverSource = new CancellationTokenSource();
        protected CancellationToken _token => _serverSource.Token;

        public WebService()
        {
            _serverSource = new CancellationTokenSource();
            _gs = SetupUtils.SetupFromConfig<LoginGameState>(this, "login", new LoginSetupService(), _token).GetAwaiter().GetResult();
            _clientService = _gs.loc.Get<IClientService>();
            _loginService = _gs.loc.Get<ILoginService>();
        }

        public async Task<string> HandleClient(string postData)
        {
            return WebUtils.PackageResults(await _clientService.HandleClient(SetupGameState(), postData, _token));
        }

        public async Task<string> HandleLogin(string postData)
        {
            return WebUtils.PackageResults(await _loginService.Login(SetupGameState(), postData, _token));
        }

        public void UpdateFromNewGameData(GameData gameData)
        {
            _gs.data = gameData;
        }

        protected LoginGameState SetupGameState()
        {
            return new LoginGameState()
            {
                config = _gs.config,
                data = _gs.data,
                repo = _gs.repo,
                loc = _gs.loc,
                rand = new MyRandom(),
                commandHandlers = _gs.commandHandlers,
                mapStubs = _gs.mapStubs,
            };
        }
    }
}
