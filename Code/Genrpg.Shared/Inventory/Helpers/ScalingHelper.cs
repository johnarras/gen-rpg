using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Stats.Entities;
using System.Threading.Tasks;
namespace Genrpg.Shared.Inventory.Helpers
{
    public class ScalingHelper : IEntityHelper
    {
        public long GetKey() { return EntityType.Scaling; }
        public string GetDataPropertyName() { return "ScalingTypes"; }

        public IIndexedGameItem Find(GameState gs, long id)
        {
            return gs.data.GetGameData<ItemSettings>().GetScalingType(id);
        }
    }
}
