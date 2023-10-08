using MessagePack;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.DataStores.GameSettings;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class SpellModifier : ChildSettings, IIndexedGameItem
    {
        public const int DefaultCostScale = 100;

        public const int Cooldown = 1;
        public const int Range = 2;
        public const int ExtraTargets = 3;
        public const int Radius = 4;
        public const int Scale = 5;
        public const int Ticks = 6;
        public const int Shots = 7;
        public const int Charges = 8;
        public const int ComboGen = 9;
        public const int ProcChance = 10;
        public const int ProcScale = 11;
        public const int CastTime = 12;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }

        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Icon { get; set; }
        [Key(5)] public string Art { get; set; }
        [Key(6)] public string Desc { get; set; }
        [Key(7)] public string DisplaySuffix { get; set; }
        [Key(8)] public string DataMemberName { get; set; }
        [Key(9)] public bool IsProcMod { get; set; }
        [Key(10)] public float DisplayMult { get; set; }

        [Key(11)] public List<SpellModifierValue> Values { get; set; }

        public SpellModifier()
        {
            Values = new List<SpellModifierValue>();
        }

        public SpellModifierValue GetDefaultValue()
        {
            if (Values == null)
            {
                return null;
            }

            return Values.FirstOrDefault(X => X.CostScale == DefaultCostScale);
        }
    }
}
