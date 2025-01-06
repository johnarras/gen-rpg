using Genrpg.Shared.BoardGame.Messages.RollDice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.MobileHUD
{
    public class MobileHUDScreen : BaseScreen
    {

        private IClientWebService _webService;

        public GButton RollButton;


        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await Task.CompletedTask;

            _uiService.SetButton(RollButton, GetName(), RollDice);
        }

        private void RollDice()
        {
            _webService.SendClientWebCommand(new RollDiceCommand(), _token);
        }
    }
}
