using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Spawns.Helpers
{
    public class SpawnHelper : BaseEntityHelper<SpawnSettings,SpawnTable>
    {
        public override long GetKey() { return EntityTypes.Spawn; }
    }
}
