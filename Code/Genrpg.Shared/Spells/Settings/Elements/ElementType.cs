using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Spells.Casting;

namespace Genrpg.Shared.Spells.Settings.Elements
{
    public class ElementFlags
    {
        public const int DisableRanks = 1 << 0;
    }

    [MessagePackObject]
    public class ElementType : ChildSettings, IIndexedGameItem, IInfo
    {

        public const int SecondaryDebuffStatDiv = 10;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public string Art { get; set; }

        [Key(7)] public string CastAnim { get; set; }

        [Key(8)] public int RankScale { get; set; }

        [Key(9)] public List<ElementSkill> Skills { get; set; }

        [Key(10)] public List<SpellProc> Procs { get; set; }

        /// <summary>
        /// Passive stats for using this element
        /// </summary>
        [Key(11)] public List<AbilityEffect> BonusEfffects { get; set; }


        [Key(12)] public List<StatPct> BuffEffects { get; set; }

        [Key(13)] public List<StatPct> DebuffEffects { get; set; }


        [Key(14)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public long GetId() { return IdKey; }
        public ElementType()
        {
            Skills = new List<ElementSkill>();
            Procs = new List<SpellProc>();
            BonusEfffects = new List<AbilityEffect>();
        }

        public string ShowInfo()
        {
            return "Element: " + Name;
        }

        public ElementSkill GetSkill(long skillTypeId)
        {
            ElementSkill ek = Skills.FirstOrDefault(x => x.SkillTypeId == skillTypeId);
            if (ek == null)
            {
                ek = new ElementSkill() { SkillTypeId = skillTypeId };
                Skills.Add(ek);
            }
            return ek;
        }

        public int GetScalePct(long skillTypeId)
        {
            return GetSkill(skillTypeId).ScalePct;
        }

        public int GetCostPct(long skillTypeId)
        {
            return GetSkill(skillTypeId).CostPct;
        }
    }
}
