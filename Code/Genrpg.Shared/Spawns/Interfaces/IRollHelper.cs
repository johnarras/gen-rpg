using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spawns.Interfaces
{
    public interface IRollHelper : ISetupDictionaryItem<long>
    {
        List<SpawnResult> Roll(GameState gs, RollData rollData, SpawnItem item);
    }
}
