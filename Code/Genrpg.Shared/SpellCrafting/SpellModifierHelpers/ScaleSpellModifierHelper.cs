using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class ScaleSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long GetKey() { return SpellModifiers.Scale; }

        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            return value / 100.0f;

        }
    }
}
