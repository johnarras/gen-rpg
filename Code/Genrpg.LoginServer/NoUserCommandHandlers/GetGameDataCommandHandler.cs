using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Website.Messages.NoUserGameData;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.NoUserCommandHandlers
{
    public class GetGameDataCommandHandler : BaseNoUserCommandHandler<NoUserGameDataCommand>
    {

        IGameDataService _gameDataService;

        protected override async Task InnerHandleMessage(WebContext context, NoUserGameDataCommand command, CancellationToken token)
        {
            NoUserGameDataResult result = new NoUserGameDataResult();
            
            result.GameData = _gameDataService.GetClientGameData(null, true);

            await Task.CompletedTask;
            context.Results.Add(result);
        }
    }
}
