using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.Settings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.SpellModifierHelpers
{
    public abstract class BaseSpellModifierHelper : ISpellModifierHelper
    {
        private IGameData _gameData = null;
        public abstract long GetKey();
        public abstract double GetCostScale(MapObject obj, double value);

        protected virtual SpellModifier GetModifier(MapObject obj)
        {
            return _gameData.Get<SpellModifierSettings>(obj).Get(GetKey());
        }

        public virtual double GetMinValue(MapObject obj)
        {
            return GetModifier(obj).MinValue;
        }

        public virtual double GetMaxValue(MapObject obj)
        {
            return GetModifier(obj).MaxValue;
        }

        public virtual double ValueDelta(MapObject obj)
        {
            return GetModifier(obj).ValueDelta;
        }

        public virtual double GetValidValue(MapObject obj, double value)
        {
            value = MathUtils.Clamp(GetMinValue(obj), value, GetMaxValue(obj));

            double minValue = GetMinValue(obj);
            double valueDelta = ValueDelta(obj);

            double divDiff = (value - minValue) / valueDelta;

            int intDivDiff = (int)(divDiff);

            value = intDivDiff * valueDelta + minValue;

            return value;

        }

        public virtual string GetInfoText(MapObject obj)
        {
            SpellModifier modifier = GetModifier(obj);

            return modifier.MinValue + " to " + modifier.MaxValue + " by " + modifier.ValueDelta;
        }

        public virtual List<double> GetValidValues(MapObject obj)
        {
            SpellModifier modifier = GetModifier(obj);
            List<double> retval = new List<double>();

            if (modifier.ValueDelta <= 0)
            {
                return retval;
            }

            double currVal = modifier.MinValue;
            do
            {
                retval.Add(currVal);
                currVal += modifier.ValueDelta;
            }
            while (currVal <= modifier.MaxValue);

            return retval;
        }
    }
}
