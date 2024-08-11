using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Inventory.Helpers
{
    public class ItemHelper : BaseEntityHelper<ItemTypeSettings,ItemType>
    {
        public override long GetKey() { return EntityTypes.Item; }
    }
}
