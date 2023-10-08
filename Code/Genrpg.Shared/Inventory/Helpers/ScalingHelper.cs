using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Stats.Entities;
using System.Threading.Tasks;
namespace Genrpg.Shared.Inventory.Helpers
{
    public class ScalingHelper : IEntityHelper
    {
        public long GetKey() { return EntityType.Scaling; }
        public string GetDataPropertyName() { return "ScalingTypes"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.data.GetGameData<ScalingTypeSettings>(obj).GetScalingType(id);
        }
    }
}
