
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Ftue.Constants;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;
using UnityEngine;

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

            TaskUtils.ForgetAwaitable(ClientStartOpen(newStep));

            return newStep;
        }

        private async Awaitable ClientStartOpen(FtueStep newStep)
        {
            // Maybe open another screen or do something else before showing the popup.


            if (newStep.FtuePopupTypeId != FtuePopupTypes.NoWindow)
            {
                _screenService.Open(ScreenId.Ftue, newStep);
            }
            await Task.CompletedTask;
        }
    }
}
