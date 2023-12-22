using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class RangeSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long GetKey() { return SpellModifiers.Range; }

        // Linear in range, max at 3x at 45 units so 
        public override double GetCostScale(GameState gs, MapObject obj, double value)
        {
            value = GetValidValue(gs, obj, value);

            return 1.0f + value * 0.05;
        }
    }
}
