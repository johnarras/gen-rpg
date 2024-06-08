using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public class ShieldEffectHandler : BaseSpellEffectHandler
    {
        public override long GetKey() { return EntityTypes.Shield; }
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }

        public override List<ActiveSpellEffect> CreateEffects(IRandom rand, SpellHit hitData)
        {
            ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
            eff.EntityTypeId = EntityTypes.Shield;
            eff.EntityId = 0;
            eff.Quantity = hitData.BaseQuantity;
            return new List<ActiveSpellEffect>() { eff };
        }

        public override bool HandleEffect(IRandom rand, ActiveSpellEffect eff)
        {
            return true;
        }
    }
}
