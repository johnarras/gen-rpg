using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Stats.Entities;
using System.Threading.Tasks;
using Genrpg.Shared.Stats.Settings.Scaling;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Inventory.Helpers
{
    public class ScalingHelper : IEntityHelper
    {
        private IGameData _gameData;
        public long GetKey() { return EntityTypes.Scaling; }
        public string GetDataPropertyName() { return "ScalingTypes"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return _gameData.Get<ScalingTypeSettings>(obj).Get(id);
        }
    }
}
