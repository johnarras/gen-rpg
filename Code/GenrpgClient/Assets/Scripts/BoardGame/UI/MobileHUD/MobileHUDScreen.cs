
using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.Lockouts.Constants;
using Assets.Scripts.Lockouts.Services;
using Genrpg.Shared.BoardGame.WebApi.RollDice;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.MobileHUD
{
    public class MobileHUDScreen : BaseScreen
    {

        private IBoardGameController _boardGameController;


        public GButton RollButton;


        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await Task.CompletedTask;

            _uiService.SetButton(RollButton, GetName(), RollDice);
        }

        private void RollDice()
        {
            _boardGameController.RollDice();
        }
    }
}
