using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Crafting.Settings.Recipes;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Crafting.Helpers
{
    public class RecipeHelper : BaseEntityHelper<RecipeSettings, RecipeType>
    {
        public override long GetKey() { return EntityTypes.Recipe; }
    }
}
