using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Spells.Entities
{
    /// <summary>
    /// A list of these gives the appropriate dropdowns allowed for each spell.
    /// </summary>
    [MessagePackObject]
    public class SpellModifierValue : IInfo
    {
        public long GetId() { return Value; }
        [Key(0)] public int Value { get; set; }
        [Key(1)] public int CostScale { get; set; }



        private SpellModifier _tempMod = null;
        public void InitTempMod(SpellModifier mod)
        {
            _tempMod = mod;
        }

        public SpellModifier GetTempMod()
        {
            return _tempMod;
        }


        public string ShowInfo()
        {
            if (_tempMod == null)
            {
                return "";
            }

            string txt = _tempMod.Name + ": ";

            if (_tempMod.IdKey == SpellModifier.CastTime)
            {
                txt += Value * SpellConstants.CastTimeTickMS / 1000.0f + " sec. ";
            }
            else if (_tempMod.IdKey == SpellModifier.Cooldown)
            {
                txt += Value + " sec.";
            }
            else if (_tempMod.IdKey == SpellModifier.Charges ||
                _tempMod.IdKey == SpellModifier.ExtraTargets ||
                _tempMod.IdKey == SpellModifier.Shots ||
                _tempMod.IdKey == SpellModifier.Ticks ||
                _tempMod.IdKey == SpellModifier.Range ||
                _tempMod.IdKey == SpellModifier.Radius)
            {
                txt += Value + " ";
            }
            else if (_tempMod.IdKey == SpellModifier.ComboGen ||
                _tempMod.IdKey == SpellModifier.ProcChance ||
                _tempMod.IdKey == SpellModifier.ProcScale ||
                _tempMod.IdKey == SpellModifier.Scale)
            {
                txt += Value + "% ";
            }
            txt += ": [" + CostScale / 100.0f + "x]";
            return txt;
        }
    }
}
