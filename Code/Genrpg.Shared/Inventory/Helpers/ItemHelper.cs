using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using System.Threading.Tasks;
namespace Genrpg.Shared.Inventory.Helpers
{
    public class ItemHelper : IEntityHelper
    {
        public long GetKey() { return EntityType.Item; }
        public string GetDataPropertyName() { return "ItemTypes"; }


        public IIndexedGameItem Find(GameState gs, long id)
        {
            return gs.data.GetGameData<ItemSettings>().GetItemType(id);
        }
    }
}
