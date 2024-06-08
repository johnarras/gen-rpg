using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using System.Xml.Schema;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public class CooldownSpellModifierHelper : BaseSpellModifierHelper
    {
        public override long GetKey() { return SpellModifiers.Cooldown; }


        double lowval = 10;
        double midval = 30;
        // Cooldown cost scale 
        public override double GetCostScale(MapObject obj, double value)
        {
            value = GetValidValue(obj, value);

            if (value < lowval)
            {
                return 1.0f - value * 0.05f;
            }

            double mult = 0.5f;

            if (value < midval)
            {
                mult -= (value - lowval) * 0.01f;
                return mult;
            }

            mult -= (value - midval) * 0.05f;

            if (mult < 0.10f)
            {
                mult = 0.10f;
            }
            return mult;
        }
    }
}
