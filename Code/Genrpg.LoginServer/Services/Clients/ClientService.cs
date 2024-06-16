using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.PlayerData;
using Genrpg.LoginServer.Services.LoginServer;
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
        private IPlayerDataService _playerDataService = null;
        private IGameDataService _gameDataService = null;
        private IRepositoryService _repoService = null;
        private ILogService _logService = null;
        private ILoginServerService _loginServerService = null;

        public async Task HandleClient(LoginContext context, string postData, CancellationToken token)
        {
            LoginServerCommandSet commandSet = SerializationUtils.Deserialize<LoginServerCommandSet>(postData);

            await LoadLoggedInPlayer(context, commandSet.UserId, commandSet.SessionId);

            try
            {
                foreach (ILoginCommand comm in commandSet.Commands)
                {
                    IClientCommandHandler handler = _loginServerService.GetCommandHandler(comm.GetType());
                    if (handler != null)
                    {
                        await handler.Execute(context, comm, token);
                    }
                }

                List<ILoginResult> errors = new List<ILoginResult>();

                foreach (ILoginResult result in context.Results)
                {
                    if (result is ErrorResult error)
                    {                        
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    context.Results.Clear();
                    context.Results.AddRange(errors);
                    return;
                }

                await SaveAll(context);
            }
            catch (Exception e)
            {
                string errorMessage = "HandleLoginCommand." + commandSet.Commands.Select(x => x.GetType().Name + " ").ToList();
                _logService.Exception(e, errorMessage);
                WebUtils.ShowError(context, errorMessage);
            }

            return;
        }

        private async Task LoadLoggedInPlayer(LoginContext context, string userId, string sessionId)
        {
            context.user = await _repoService.Load<User>(userId);

            if (context.user == null || context.user.SessionId != sessionId)
            {
                return;
            }

            if (!string.IsNullOrEmpty(context.user.CurrCharId))
            {
                context.coreCh = await _repoService.Load<CoreCharacter>(context.user.CurrCharId);

                if (context.coreCh != null)
                {
                    context.ch = new Character(_repoService);
                    CharacterUtils.CopyDataFromTo(context.coreCh, context.ch);

                    await context.ch.GetAsync<GameDataOverrideData>(context);

                    RefreshGameSettingsResult result = _gameDataService.GetNewGameDataUpdates(context.ch, false);

                    if (result != null)
                    {
                        context.Results.Add(result);
                    }
                }
            }

            return;
        }

        private async Task SaveAll(LoginContext context)
        {
            if (context.user != null)
            {
                await _repoService.Save(context.user);
            }
            _playerDataService.SavePlayerData(context.ch, true);
        }

    }
}
