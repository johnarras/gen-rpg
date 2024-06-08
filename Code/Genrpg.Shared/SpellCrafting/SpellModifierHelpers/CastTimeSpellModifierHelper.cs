using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class CastTimeSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long GetKey() { return SpellModifiers.CastTime; }



        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            return MathUtils.Clamp(0.25, 1.0f - value * 0.1f, 1);
        }
    }
}
