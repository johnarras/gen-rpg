using Genrpg.Shared.GameSettings.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayerFiltering.Interfaces
{
    public interface IFilteredObject
    {
        string GetName(string docType);
        void SetGameDataOverrides(GameDataOverrideList list);
    }
}
