using Cysharp.Threading.Tasks;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Ftue.Constants;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace Assets.Scripts.Ftue.Services
{
    public class ClientFtueService : FtueService
    {
        IScreenService _screenService = null;

        public override FtueStep StartStep(IRandom random, Character ch, long ftueStepId)
        {
            FtueStep newStep = base.StartStep(random ,ch, ftueStepId);

            if (newStep == null)
            {
                return null;
            }

            ClientStartOpen(newStep).Forget();

            return newStep;
        }

        private async UniTask ClientStartOpen(FtueStep newStep)
        {
            await UniTask.CompletedTask;

            // Maybe open another screen or do something else before showing the popup.


            if (newStep.FtuePopupTypeId != FtuePopupTypes.NoWindow)
            {
                _screenService.Open(ScreenId.Ftue, newStep);
            }
        }
    }
}
