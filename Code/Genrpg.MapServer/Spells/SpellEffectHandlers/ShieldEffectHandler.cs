using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public class ShieldEffectHandler : BaseSpellEffectHandler
    {
        public override long GetKey() { return EntityTypes.Shield; }
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }

        public override List<ActiveSpellEffect> CreateEffects(GameState gs, SpellHit hitData)
        {
            ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
            eff.EntityTypeId = EntityTypes.Shield;
            eff.EntityId = 0;
            eff.Quantity = hitData.BaseQuantity;
            return new List<ActiveSpellEffect>() { eff };
        }

        public override bool HandleEffect(GameState gs, ActiveSpellEffect eff)
        {
            return true;
        }
    }
}
