using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.Spawns.Settings;

namespace Genrpg.Shared.Spawns.Helpers
{
    public class SpawnHelper : IEntityHelper
    {

        public long GetKey() { return EntityTypes.Spawn; }
        public string GetDataPropertyName() { return "SpawnTables"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.data.GetGameData<SpawnSettings>(obj).GetSpawnTable(id);
        }
    }
}
