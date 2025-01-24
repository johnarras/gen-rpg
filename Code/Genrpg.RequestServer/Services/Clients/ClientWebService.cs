using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.RequestServer.ClientUser.RequestHandlers;

namespace Genrpg.RequestServer.Services.Clients
{
    public class ClientWebService : IClientWebService
    {
        private IGameDataService _gameDataService = null;
        private IRepositoryService _repoService = null;
        private ILogService _logService = null;
        private IWebServerService _loginServerService = null;
        private IHourlyUpdateService _hourlyUpdateService = null;

        public async Task HandleUserClientRequest(WebContext context, string postData, CancellationToken token)
        {
            WebServerRequestSet commandSet = SerializationUtils.Deserialize<WebServerRequestSet>(postData);

            await LoadLoggedInPlayer(context, commandSet.UserId, commandSet.SessionId);

            try
            {
                foreach (IWebRequest comm in commandSet.Requests)
                {
                    IClientUserRequestHandler handler = _loginServerService.GetClientCommandHandler(comm.GetType());
                    if (handler != null)
                    {
                        await handler.Execute(context, comm, token);
                    }
                }

                List<IWebResponse> errors = new List<IWebResponse>();

                foreach (IWebResponse response in context.Responses)
                {
                    if (response is ErrorResponse error)
                    {
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    context.Responses.Clear();
                    context.Responses.AddRange(errors);
                    return;
                }

                await SaveAll(context);
            }
            catch (Exception e)
            {
                string errorMessage = "HandleLoginCommand." + commandSet.Requests.Select(x => x.GetType().Name + " ").ToList();
                _logService.Exception(e, errorMessage);
                context.ShowError(errorMessage);
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
            await _hourlyUpdateService.CheckHourlyUpdate(context);

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
