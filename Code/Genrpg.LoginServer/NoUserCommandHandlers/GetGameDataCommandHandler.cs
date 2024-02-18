using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Login.Messages.NoUserGameData;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.NoUserCommandHandlers
{
    public class GetGameDataCommandHandler : BaseNoUserCommandHandler<NoUserGameDataCommand>
    {

        IGameDataService _gameDataService;

        protected override async Task InnerHandleMessage(LoginGameState gs, NoUserGameDataCommand command, CancellationToken token)
        {
            NoUserGameDataResult result = new NoUserGameDataResult();
            
            result.GameData = _gameDataService.GetClientGameData(gs, null, true);

            await Task.CompletedTask;
            gs.Results.Add(result);
        }
    }
}
