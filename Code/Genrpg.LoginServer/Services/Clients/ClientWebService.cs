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
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Website.Messages.RefreshGameSettings;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;
using Microsoft.Azure.Amqp.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.LoginServer.ClientCommandHandlers;

namespace Genrpg.LoginServer.Services.Clients
{
    public class ClientWebService : IClientWebService
    {
        private IPlayerDataService _playerDataService = null;
        private IGameDataService _gameDataService = null;
        private IRepositoryService _repoService = null;
        private ILogService _logService = null;
        private IWebServerService _loginServerService = null;

        public async Task HandleWebCommand(WebContext context, string postData, CancellationToken token)
        {
            WebServerCommandSet commandSet = SerializationUtils.Deserialize<WebServerCommandSet>(postData);

            await LoadLoggedInPlayer(context, commandSet.UserId, commandSet.SessionId);

            try
            {
                foreach (IWebCommand comm in commandSet.Commands)
                {
                    IClientCommandHandler handler = _loginServerService.GetClientCommandHandler(comm.GetType());
                    if (handler != null)
                    {
                        await handler.Execute(context, comm, token);
                    }
                }

                List<IWebResult> errors = new List<IWebResult>();

                foreach (IWebResult result in context.Results)
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

        private async Task LoadLoggedInPlayer(WebContext context, string userId, string sessionId)
        {
            context.user = await _repoService.Load<User>(userId);

            if (context.user == null || context.user.SessionId != sessionId)
            {
                return;
            }

            context.Add(context.user);

            return;
        }

        private async Task SaveAll(WebContext context)
        {
            if (context.user != null)
            {
                await _repoService.Save(context.user);
            }

            List<IStringId> unitDataList = context.GetAllData();

            List<Task> saveTasks = new List<Task>();

            foreach (IUnitData unitData in  unitDataList)
            {
                saveTasks.Add(_repoService.Save(unitData));
            }
               
            await Task.WhenAll(saveTasks);  
        }

    }
}
