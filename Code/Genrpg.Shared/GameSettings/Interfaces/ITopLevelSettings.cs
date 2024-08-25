using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.GameSettings.Interfaces
{
    public interface ITopLevelSettings : IGameSettings
    {
        void SetupForEditor();
    }
}
