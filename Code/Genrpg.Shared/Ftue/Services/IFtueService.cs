using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Ftue.Settings.Steps;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Ftue.Services
{
    public interface IFtueService : IInitializable
    {
        bool IsComplete(GameState gs, Character ch);
        FtueStep GetCurrentStep(GameState gs, Character ch);
        FtueStep StartStep(GameState gs, Character ch, long ftueStepId);
        bool CanClickButton(GameState gs, Character ch, string screenName, string buttonName);
        void CompleteStep(GameState gs, Character ch, long ftueStepId);
    }
}
