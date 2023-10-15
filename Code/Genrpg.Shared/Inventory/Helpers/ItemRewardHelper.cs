using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Spawns.Interfaces;
using System.Threading.Tasks;
namespace Genrpg.Shared.Inventory.Helpers
{
    public class ItemRewardHelper : IRewardHelper
    {
        private IInventoryService _inventoryService = null;
        public bool GiveReward(GameState gs, Character ch, long entityId, long quantity, object extraData = null)
        {
            Item startItem = extraData as Item;
            if (startItem != null)
            {
                _inventoryService.AddItem(gs, ch, startItem, true);
                return true;
            }
            return true;
        }

        public long GetKey() { return EntityTypes.Item; }

    }
}
