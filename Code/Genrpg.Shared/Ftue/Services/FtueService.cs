using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Ftue.PlayerData;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Ftue.Services
{
    public class FtueService : IFtueService
    {

        private IGameData _gameData = null;

        public FtueStep GetCurrentStep(IRandom rand, Character ch)
        {
            FtueData ftueData = ch.Get<FtueData>();

            return _gameData.Get<FtueStepSettings>(ch).Get(ftueData.CurrentFtueStepId);
        }

        public bool CanClickButton(IRandom rand, Character ch, string screenName, string buttonName)
        {
            return true;
        }

        public void CompleteStep(IRandom rand, Character ch, long FtueStepId)
        {
        }

        public long GetNextFtueStepId(IRandom rand, Character ch)
        {
            return 0;
        }

        public virtual FtueStep StartStep(IRandom rand, Character ch, long ftueStepId)
        {
            FtueData ftueData = ch.Get<FtueData>();

            FtueStep ftueStep = _gameData.Get<FtueStepSettings>(ch).Get(ftueStepId);

            if (ftueStep == null)
            {
                ftueData.CurrentFtueStepId = 0;
            }
            else
            {
                ftueData.CurrentFtueStepId = ftueStepId;
            }
            return ftueStep;
        }

        public bool IsComplete(IRandom rand, Character ch)
        {
            return true;
        }

        public void SetCurrentStep(IRandom rand, Character ch, long ftueStepId)
        {
            FtueData ftueData = ch.Get<FtueData>();

            ftueData.CurrentFtueStepId = ftueStepId;
        }
    }
}
