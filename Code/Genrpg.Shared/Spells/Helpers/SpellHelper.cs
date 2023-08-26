using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Entities;
using System.Threading.Tasks;

namespace Genrpg.Shared.Spells.Helpers
{

    public class SpellHelper : IEntityHelper
    {
        public long GetKey() { return EntityType.Spell; }
        public string GetDataPropertyName() { return "Spells"; }

        public IIndexedGameItem Find(GameState gs, long id)
        {
            return gs.data.GetGameData<SpellSettings>().GetSpell(id);
        }
    }
}
