using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class MaxChargesSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long GetKey() { return SpellModifiers.MaxCharges; }

        public override double GetCostScale(GameState gs, MapObject obj, double value)
        {
            value = GetValidValue(gs, obj, value);

            if (value < 1)
            {
                return 1.0f;
            }

            return 1.0f + Math.Sqrt(value);
        }
    }
}
