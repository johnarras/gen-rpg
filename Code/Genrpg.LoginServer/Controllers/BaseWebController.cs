
using Genrpg.ServerShared;
using Genrpg.ServerShared.Config;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.Setup;
using Genrpg.LoginServer.Setup;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.ServerShared.Maps;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Login.Messages.Error;
using Genrpg.ServerShared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings;

namespace Genrpg.LoginServer.Controllers
{
    public class BaseWebController : ControllerBase, IGameDataContainer
    {
        private static LoginGameState _gs;

        protected LoginGameState gs { get; set; }

        protected async Task SetupGameState(CancellationToken token)
        {
            if (_gs == null)
            {
                _gs = await SetupUtils.SetupFromConfig<LoginGameState>(this, "login", new LoginSetupService(), this, token);                
            }

            gs = new LoginGameState()
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

        public void UpdateFromNewGameData(GameData gameData)
        {
            _gs.data = gameData;
            _gs.logger.Message("UpdateFromNewGameData");
        }

        protected string PackageResults(List<ILoginResult> results)
        {
            return SerializationUtils.Serialize(new LoginServerResultSet() { Results = results });
        }

        protected async Task<List<ILoginCommand>> LoadSessionCommands(string data, CancellationToken token)
        {
            await SetupGameState(token);

            gs.loc.Resolve(this);

            LoginServerCommandSet commands = SerializationUtils.Deserialize<LoginServerCommandSet>(data);

            await LoadLoggedInUser(gs, commands.UserId, commands.SessionId);

            return commands.Commands;
        }

        public async Task<User> LoadLoggedInUser(LoginGameState gs, string userId, string sessionId)
        {
            gs.user = await gs.repo.Load<User>(userId);

            if (gs.user == null || gs.user.SessionId != sessionId)
            {
                return null;
            }
            
            return gs.user;
        }

        public void ShowError(LoginGameState gs, string msg)
        {
            gs.Results.Add(new ErrorResult() { Error = msg });
        }
    }
}
