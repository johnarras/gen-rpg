using Genrpg.RequestServer.Core;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Website.Messages.Login;
using Genrpg.Shared.Website.Messages.NoUserGameData;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.NoUserCommands
{
    public class GetGameDataCommandHandler : BaseNoUserCommandHandler<NoUserGameDataCommand>
    {

        IGameDataService _gameDataService = null!;

        protected override async Task InnerHandleMessage(WebContext context, NoUserGameDataCommand command, CancellationToken token)
        {
            NoUserGameDataResult result = new NoUserGameDataResult();

            List<ITopLevelSettings> topLevelSettings = _gameDataService.GetClientGameData(null, true);
            result.GameData = _gameDataService.MapToApi(context.user, topLevelSettings);

            await Task.CompletedTask;
            context.Results.Add(result);
        }
    }
}
