using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Crafting.Settings.Recipes;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Crafting.Helpers
{

    public class RecipeHelper : IEntityHelper
    {
        private IGameData _gameData;
        public long GetKey() { return EntityTypes.Recipe; }
        public string GetDataPropertyName() { return "Recipes"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return _gameData.Get<RecipeSettings>(obj).Get(id);
        }
    }
}
