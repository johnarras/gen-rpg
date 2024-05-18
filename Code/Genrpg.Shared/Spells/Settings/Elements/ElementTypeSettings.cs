using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.Spells.Procs.Entities;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Stats.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        [Key(7)] public string CasterActionName { get; set; }
        [Key(8)] public string ObserverActionName { get; set; }

        [Key(9)] public string CastAnim { get; set; }

        [Key(10)] public long VulnElementTypeId { get; set; }

        [Key(11)] public List<ElementSkill> Skills { get; set; } = new List<ElementSkill>();

        [Key(12)] public List<OldSpellProc> OldProcs { get; set; } = new List<OldSpellProc>();

      

        /// <summary>
        /// Passive stats for using this element
        /// </summary>
        [Key(13)] public List<AbilityEffect> BonusEffects { get; set; } = new List<AbilityEffect>();


        [Key(14)] public List<StatPct> BuffEffects { get; set; } = new List<StatPct>();

        [Key(15)] public List<StatPct> DebuffEffects { get; set; } = new List<StatPct>();


        [Key(16)] public List<SpellProc> Procs { get; set; } = new List<SpellProc>();


        [Key(17)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public long GetId() { return IdKey; }
       
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


    [MessagePackObject]
    public class ElementSkill
    {
        [Key(0)] public long SkillTypeId { get; set; }
        /// <summary>
        /// Percent cost to use this skill with this element. 100 = normal
        /// </summary>
        [Key(1)] public int CostPct { get; set; }
        /// <summary>
        /// Percent damage/healing/statmodifier to use this skill with this element. 100 = normal
        /// </summary>
        [Key(2)] public int ScalePct { get; set; }

        [Key(3)] public long OverrideEntityTypeId { get; set; }
        [Key(4)] public long OverrideEntityId { get; set; }

        public ElementSkill()
        {
            CostPct = 100;
            ScalePct = 100;
        }
    }


    [MessagePackObject]
    public class ElementTypeSettings : ParentSettings<ElementType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class ElementTypeSettingsApi : ParentSettingsApi<ElementTypeSettings, ElementType> { }
    [MessagePackObject]
    public class ElementTypeSettingsLoader : ParentSettingsLoader<ElementTypeSettings, ElementType> { }

    [MessagePackObject]
    public class ElementTypeSettingsMapper : ParentSettingsMapper<ElementTypeSettings, ElementType, ElementTypeSettingsApi> { }



}
