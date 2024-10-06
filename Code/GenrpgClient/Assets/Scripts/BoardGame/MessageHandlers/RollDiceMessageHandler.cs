using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.BoardGame.Messages.RollDice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.BoardGame.MessageHandlers
{
    public class RollDiceMessageHandler : BaseClientLoginResultHandler<RollDiceResult>
    {
        private IBoardGameController _boardGameController;
        protected override void InnerProcess(RollDiceResult result, CancellationToken token)
        {
            _logService.Info("TileIndex: " + result.NextBoard.TileIndex);
            _boardGameController.ShowDiceRoll(result);
        }
    }
}
