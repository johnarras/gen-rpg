using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class DurationSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long GetKey() { return SpellModifiers.Duration; }

        public override double GetCostScale(GameState gs, MapObject obj, double value)
        {
            value = GetValidValue(gs, obj, value);

            double scale = 1.0f;

            if (value > 0)
            {
                scale++;
            }
            if (value > 1)
            {
                scale += 0.9;
            }
            if (value > 2)
            {
                scale += 0.8;
            }
            if (value > 3)
            {
                scale += (value - 3) * 0.7;
            }

            return scale;
        }
    }
}
