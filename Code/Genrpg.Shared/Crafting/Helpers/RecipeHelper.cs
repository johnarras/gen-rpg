using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Crafting.Settings.Recipes;

namespace Genrpg.Shared.Crafting.Helpers
{

    public class RecipeHelper : IEntityHelper
    {
        public long GetKey() { return EntityTypes.Recipe; }
        public string GetDataPropertyName() { return "Recipes"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.data.GetGameData<RecipeSettings>(obj).GetRecipeType(id);
        }
    }
}
