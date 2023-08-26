using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Spells.Entities
{
    public class ElementFlags
    {
        public const int DisableRanks = 1 << 0;
    }

    [MessagePackObject]
    public class ElementType : IIndexedGameItem, IInfo
    {

        public const int SecondaryDebuffStatDiv = 10;

        
        [Key(0)] public long IdKey { get; set; }

        public long GetId() { return IdKey; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }

        [Key(4)] public string Art { get; set; }

        [Key(5)] public string CastAnim { get; set; }

        [Key(6)] public int RankScale { get; set; }

        [Key(7)] public List<ElementSkill> Skills { get; set; }

        [Key(8)] public List<SpellProc> Procs { get; set; }

        /// <summary>
        /// Passive stats for using this element
        /// </summary>
        [Key(9)] public List<AbilityEffect> BonusEfffects { get; set; }


        [Key(10)] public List<StatPct> BuffEffects { get; set; }

        [Key(11)] public List<StatPct> DebuffEffects { get; set; }


        [Key(12)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

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
