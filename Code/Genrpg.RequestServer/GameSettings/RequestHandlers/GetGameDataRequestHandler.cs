using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.NoUserRequests.RequestHandlers;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Website.Messages.NoUserGameData;

namespace Genrpg.RequestServer.GameSettings.RequestHandlers
{
    public class GetGameDataRequestHandler : BaseNoUserRequestHandler<NoUserGameDataRequest>
    {

        IGameDataService _gameDataService = null!;

        protected override async Task HandleRequestInternal(WebContext context, NoUserGameDataRequest request, CancellationToken token)
        {
            NoUserGameDataResponse response = new NoUserGameDataResponse();

            List<ITopLevelSettings> topLevelSettings = _gameDataService.GetClientGameData(null, true);
            response.GameData = _gameDataService.MapToApi(context.user, topLevelSettings);

            await Task.CompletedTask;
            context.Responses.Add(response);
        }
    }
}
