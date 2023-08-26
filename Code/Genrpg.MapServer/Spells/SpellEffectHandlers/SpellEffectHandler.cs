using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public class SpellEffectHandler : BaseSpellEffectHandler
    {
        public override long GetKey() { return EntityType.Spell; }
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return false; }

        public override List<SpellEffect> CreateEffects(GameState gs, SpellHit hitData)
        {
            // Used for special spells that do unique things.

            return new List<SpellEffect>();


        }

        public override bool HandleEffect(GameState gs, SpellEffect eff)
        {
            return true;
        }
    }
}
