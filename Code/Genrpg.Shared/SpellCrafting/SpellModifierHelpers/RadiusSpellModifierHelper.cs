using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class RadiusSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long GetKey() { return SpellModifiers.Radius; }

        const double _radiusDiv = 3;

        // Square of radius/3?
        public override double GetCostScale(GameState gs, MapObject obj, double value)
        {
            value = GetValidValue(gs, obj, value);

            value /= _radiusDiv;

            return 1 + value * value;

        }
    }
}
