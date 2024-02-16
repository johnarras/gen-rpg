using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.Spells.Settings.Spells;

namespace Genrpg.Shared.Spells.Helpers
{

    public class SpellHelper : IEntityHelper
    {
        public long GetKey() { return EntityTypes.Spell; }
        public string GetDataPropertyName() { return "Spells"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.data.Get<SpellTypeSettings>(obj).Get(id);
        }
    }
}
