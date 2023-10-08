using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.Interfaces
{
    public interface IGameSettings : IStringId
    {
        void AddTo(GameData gameData);
        void SetInternalIds();
    }
}
