using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spawns.Interfaces
{
    public interface IRollHelper : ISetupDictionaryItem<long>
    {
        List<SpawnResult> Roll<SI>(IRandom rand, RollData rollData, SI item) where SI : ISpawnItem;
    }
}
