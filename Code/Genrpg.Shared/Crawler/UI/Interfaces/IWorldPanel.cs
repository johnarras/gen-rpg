﻿using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.UI.Interfaces
{
    public interface IWorldPanel
    {
        void SetPicture(string image, bool useBGOnly);

        void ApplyEffect(string effectName, float duration);

        void UpdateCombatGroups();
    }
}
