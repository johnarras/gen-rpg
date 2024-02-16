using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Ftue.PlayerData;
using Genrpg.Shared.Ftue.Settings.Steps;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Ftue.Services
{
    public class FtueService : IFtueService
    {

        public async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public FtueStep GetCurrentStep(GameState gs, Character ch)
        {
            FtueData ftueData = ch.Get<FtueData>();

            return gs.data.Get<FtueStepSettings>(ch).Get(ftueData.CurrentFtueStepId);
        }

        public bool CanClickButton(GameState gs, Character ch, string screenName, string buttonName)
        {
            return true;
        }

        public void CompleteStep(GameState gs, Character ch, long FtueStepId)
        {
        }

        public long GetNextFtueStepId(GameState gs, Character ch)
        {
            return 0;
        }

        public virtual FtueStep StartStep(GameState gs, Character ch, long ftueStepId)
        {
            FtueData ftueData = ch.Get<FtueData>();

            FtueStep ftueStep = gs.data.Get<FtueStepSettings>(ch).Get(ftueStepId);

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

        public bool IsComplete(GameState gs, Character ch)
        {
            return true;
        }

        public void SetCurrentStep(GameState gs, Character ch, long ftueStepId)
        {
            FtueData ftueData = ch.Get<FtueData>();

            ftueData.CurrentFtueStepId = ftueStepId;
        }
    }
}
