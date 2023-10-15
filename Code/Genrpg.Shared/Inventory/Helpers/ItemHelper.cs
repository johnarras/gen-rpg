using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
namespace Genrpg.Shared.Inventory.Helpers
{
    public class ItemHelper : IEntityHelper
    {
        public long GetKey() { return EntityTypes.Item; }
        public string GetDataPropertyName() { return "ItemTypes"; }


        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.data.GetGameData<ItemTypeSettings>(obj).GetItemType(id);
        }
    }
}
