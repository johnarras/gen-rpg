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
        public override long GetKey() { return EntityTypes.Spell; }
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return false; }

        public override List<ActiveSpellEffect> CreateEffects(GameState gs, SpellHit hitData)
        {
            // Used for special spells that do unique things.

            return new List<ActiveSpellEffect>();


        }

        public override bool HandleEffect(GameState gs, ActiveSpellEffect eff)
        {
            return true;
        }
    }
}
