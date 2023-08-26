using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Genrpg.Shared.Spawns.Helpers
{
    public class SpawnHelper : IEntityHelper
    {

        public long GetKey() { return EntityType.Spawn; }
        public string GetDataPropertyName() { return "SpawnTables"; }

        public IIndexedGameItem Find(GameState gs, long id)
        {
            return gs.data.GetGameData<SpawnSettings>().GetSpawnTable(id);
        }
    }
}
