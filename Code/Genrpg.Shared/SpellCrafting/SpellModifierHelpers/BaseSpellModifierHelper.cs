using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Entities;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public abstract class BaseSpellModifierHelper : ISpellModifierHelper
    {
        public abstract long GetKey();
        public abstract double GetCostScale(GameState gs, MapObject obj, double value);

        protected virtual SpellModifier GetModifier(GameState gs, MapObject obj)
        {
            return gs.data.GetGameData<SpellModifierSettings>(obj).GetSpellModifier(GetKey());
        }

        public virtual double GetMinValue(GameState gs, MapObject obj)
        {
            return GetModifier(gs, obj).MinValue;
        }

        public virtual double GetMaxValue(GameState gs, MapObject obj)
        {
            return GetModifier(gs, obj).MaxValue;
        }

        public virtual double ValueDelta(GameState gs, MapObject obj)
        {
            return GetModifier(gs, obj).ValueDelta;
        }

        public virtual double GetValidValue(GameState gs, MapObject obj, double value)
        {
            value = MathUtils.Clamp(GetMinValue(gs, obj), value, GetMaxValue(gs, obj));

            double minValue = GetMinValue(gs, obj);
            double valueDelta = ValueDelta(gs, obj);

            double divDiff = (value - minValue) / valueDelta;

            int intDivDiff = (int)(divDiff);

            value = intDivDiff * valueDelta + minValue;

            return value;

        }

        public virtual string GetInfoText(GameState gs, MapObject? obj)
        {
            SpellModifier modifier = GetModifier(gs, obj);

            return "Range: " + modifier.MinValue + " to " + modifier.MaxValue + " Incr: " + modifier.ValueDelta;
        }
    }
}
