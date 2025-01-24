using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.BoardGame.WebApi.RollDice;
using System.Threading;

namespace Assets.Scripts.BoardGame.MessageHandlers
{
    public class RollDiceMessageHandler : BaseClientLoginResultHandler<RollDiceResponse>
    {
        private IBoardGameController _boardGameController;
        protected override void InnerProcess(RollDiceResponse result, CancellationToken token)
        {
            _logService.Info("TileIndex: " + result.NextBoard.TileIndex);
            _boardGameController.ShowDiceRoll(result);
        }
    }
}
