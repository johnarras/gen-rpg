using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Units.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loaders
{
    public class SettingsOverrideDataLoader : UnitDataLoader<GameDataOverrideData>
    {
        public override bool SendToClient() { return false; }
    }
}
