using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Ftue.Services
{
    public interface IFtueService : IInitializable
    {
        bool IsComplete(IRandom rand, Character ch);
        FtueStep GetCurrentStep(IRandom rand, Character ch);
        FtueStep StartStep(IRandom rand, Character ch, long ftueStepId);
        bool CanClickButton(IRandom rand, Character ch, string screenName, string buttonName);
        void CompleteStep(IRandom rand, Character ch, long ftueStepId);
    }
}
