using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Spells.Helpers
{

    public class SpellHelper : BaseEntityHelper<SpellTypeSettings,SpellType>
    {
        public override long GetKey() { return EntityTypes.Spell; }
    }
}
