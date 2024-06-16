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

        protected override async Task InnerHandleMessage(LoginContext context, NoUserGameDataCommand command, CancellationToken token)
        {
            NoUserGameDataResult result = new NoUserGameDataResult();
            
            result.GameData = _gameDataService.GetClientGameData(null, true);

            await Task.CompletedTask;
            context.Results.Add(result);
        }
    }
}
