using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crafting.Helpers
{

    public class RecipeHelper : IEntityHelper
    {
        public long GetKey() { return EntityType.Recipe; }
        public string GetDataPropertyName() { return "Recipes"; }

        public IIndexedGameItem Find(GameState gs, long id)
        {
            return gs.data.GetGameData<CraftingSettings>().GetRecipeType(id);
        }
    }
}
