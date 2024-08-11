using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;
namespace Genrpg.Shared.Inventory.Helpers
{
    public class ItemRewardHelper : IRewardHelper
    {
        private IInventoryService _inventoryService = null;
        public bool GiveReward(IRandom rand, Unit unit, long entityId, long quantity, object extraData = null)
        {
            Item startItem = extraData as Item;
            if (startItem != null)
            {
                _inventoryService.AddItem(unit, startItem, true);
                return true;
            }
            return true;
        }

        public long GetKey() { return EntityTypes.Item; }

    }
}
