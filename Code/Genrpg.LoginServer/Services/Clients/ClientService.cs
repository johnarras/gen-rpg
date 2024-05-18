using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.PlayerData;
using Genrpg.LoginServer.Utils;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages;
using Genrpg.Shared.Login.Messages.Error;
using Genrpg.Shared.Login.Messages.RefreshGameSettings;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Microsoft.Azure.Amqp.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Clients
{
    public class ClientService : IClientService
    {
        public async Task Initialize(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private IPlayerDataService _playerDataService = null;
        private IGameDataService _gameDataService = null;
        private IRepositoryService _repoService = null;
        private ILogService _logService = null;

        public async Task<List<ILoginResult>> HandleClient(LoginGameState gs, string postData, CancellationToken token)
        {
            LoginServerCommandSet commandSet = SerializationUtils.Deserialize<LoginServerCommandSet>(postData);

            await LoadLoggedInPlayer(gs, commandSet.UserId, commandSet.SessionId);

            try
            {
                foreach (ILoginCommand comm in commandSet.Commands)
                {
                    if (gs.commandHandlers.TryGetValue(comm.GetType(), out IClientCommandHandler handler))
                    {
                        await handler.Execute(gs, comm, token);
                    }
                }

                List<ILoginResult> errors = new List<ILoginResult>();

                foreach (ILoginResult result in gs.Results)
                {
                    if (result is ErrorResult error)
                    {
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    return errors;
                }

                await SaveAll(gs);
            }
            catch (Exception e)
            {
                string errorMessage = "HandleLoginCommand." + commandSet.Commands.Select(x => x.GetType().Name + " ").ToList();
                _logService.Exception(e, errorMessage);
                WebUtils.ShowError(gs, errorMessage);
            }

            return gs.Results;
        }

        private async Task LoadLoggedInPlayer(LoginGameState gs, string userId, string sessionId)
        {
            gs.user = await _repoService.Load<User>(userId);

            if (gs.user == null || gs.user.SessionId != sessionId)
            {
                return;
            }

            if (!string.IsNullOrEmpty(gs.user.CurrCharId))
            {
                gs.coreCh = await _repoService.Load<CoreCharacter>(gs.user.CurrCharId);

                if (gs.coreCh != null)
                {
                    gs.ch = new Character(_repoService);
                    CharacterUtils.CopyDataFromTo(gs.coreCh, gs.ch);

                    await gs.ch.GetAsync<GameDataOverrideData>(gs);

                    RefreshGameSettingsResult result = _gameDataService.GetNewGameDataUpdates(gs, gs.ch, false);

                    if (result != null)
                    {
                        gs.Results.Add(result);
                    }
                }
            }

            return;
        }

        private async Task SaveAll(LoginGameState gs)
        {
            if (gs.user != null)
            {
                await _repoService.Save(gs.user);
            }
            _playerDataService.SavePlayerData(gs.ch, true);
        }

    }
}
