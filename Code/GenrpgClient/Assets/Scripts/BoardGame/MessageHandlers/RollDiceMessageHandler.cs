using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.Lockouts.Constants;
using Assets.Scripts.Lockouts.Services;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.BoardGame.WebApi.RollDice;
using System.Threading;

namespace Assets.Scripts.BoardGame.MessageHandlers
{
    public class RollDiceMessageHandler : BaseClientLoginResultHandler<RollDiceResponse>
    {
        private IBoardGameController _boardGameController;
        private ILockoutManager _lockoutManager;
        protected override void InnerProcess(RollDiceResponse result, CancellationToken token)
        {
            _lockoutManager.RemoveLock(LockoutTypes.RollDice, RollDiceLocks.SendRequest);
            _boardGameController.ShowDiceRoll(result);
        }
    }
}
