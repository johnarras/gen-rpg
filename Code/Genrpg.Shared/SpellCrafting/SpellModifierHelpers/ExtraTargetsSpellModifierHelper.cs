using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class ExtraTargetsSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long GetKey() { return SpellModifiers.ExtraTargets; }


        const double _extraTargetMult = 0.9f;

        // Scaling is just linear, but smaller percent. 0.9?

        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            return 1.0 + value * _extraTargetMult;
        }
    }
}
