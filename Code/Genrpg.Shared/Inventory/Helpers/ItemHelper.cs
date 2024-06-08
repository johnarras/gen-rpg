using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Inventory.Helpers
{
    public class ItemHelper : IEntityHelper
    {
        private IGameData _gameData = null;
        public long GetKey() { return EntityTypes.Item; }
        public string GetDataPropertyName() { return "ItemTypes"; }


        public IIndexedGameItem Find(IFilteredObject obj, long id)
        {
            return _gameData.Get<ItemTypeSettings>(obj).Get(id);
        }
    }
}
