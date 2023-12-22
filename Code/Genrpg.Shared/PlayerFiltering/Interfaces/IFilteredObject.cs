using Genrpg.Shared.GameSettings.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayerFiltering.Interfaces
{
    public interface IFilteredObject
    {
        string GetGameDataName(string docType);
        void SetGameDataOverrides(GameDataOverrideList list);
    }
}
