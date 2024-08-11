using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.RequestServer.Utils;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.ClientCommandHandlers;
using Genrpg.Shared.Users.PlayerData;

namespace Genrpg.RequestServer.Services.Clients
{
    public class ClientWebService : IClientWebService
    {
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
            await context.LoadUser(userId);

            if (context.user == null || context.user.SessionId != sessionId)
            {
                return;
            }

            _gameDataService.SetGameDataOverrides(context.user, false);

            return;
        }

        private async Task SaveAll(WebContext context)
        {
            if (context.user != null)
            {
                await _repoService.Save(context.user);
            }

            List<IUnitData> unitDataList = context.GetAllData();

            List<Task> saveTasks = new List<Task>();

            foreach (IUnitData unitData in unitDataList)
            {
                saveTasks.Add(_repoService.Save(unitData));
            }

            await Task.WhenAll(saveTasks);
        }

    }
}
