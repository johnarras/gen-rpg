using Genrpg.Shared.GameSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings.Interfaces
{
    public interface IGameDataContainer
    {
        void UpdateFromNewGameData(GameData gameData);
    }
}
