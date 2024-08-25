using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    public abstract class TopLevelGameSettings : BaseGameSettings, ITopLevelSettings
    {
        public override void AddTo(GameData gameData)
        {
            gameData.Set(this);
        }

        public virtual void SetupForEditor()
        {

        }
    }
}
