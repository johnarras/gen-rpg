using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;

namespace Genrpg.Shared.Spells.Settings.Spells
{
    [MessagePackObject]
    public class SpellType : ChildSettings, ISpell
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }

        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public long ElementTypeId { get; set; }
        [Key(8)] public long PowerStatTypeId { get; set; }
        [Key(9)] public int PowerCost { get; set; }
        [Key(10)] public int Cooldown { get; set; }
        [Key(11)] public float CastTime { get; set; }
        [Key(12)] public int MinRange { get; set; } = SpellConstants.MinRange;
        [Key(13)] public int MaxRange { get; set; } = SpellConstants.MaxRange;
        [Key(14)] public int MaxCharges { get; set; }
        [Key(15)] public int Shots { get; set; }

        [Key(16)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        [Key(17)] public List<SpellEffect> Effects { get; set; } = new List<SpellEffect>();

        public SpellType()
        {
        }
    }


    [MessagePackObject]
    public class SpellEffect
    {
        [Key(0)] public long SkillTypeId { get; set; } = 1;
        [Key(1)] public long EntityTypeId { get; set; }
        [Key(2)] public long EntityId { get; set; }
        [Key(3)] public int Radius { get; set; }
        [Key(4)] public int Duration { get; set; }
        [Key(5)] public int ExtraTargets { get; set; }
        [Key(6)] public int Scale { get; set; }
        [Key(7)] public int Flags { get; set; }
        [Key(8)] public string Name { get; set; }

        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }
    }

    [MessagePackObject]
    public class SpellTypeSettings : ParentSettings<SpellType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class SpellTypeSettingsApi : ParentSettingsApi<SpellTypeSettings, SpellType> { }
    [MessagePackObject]
    public class SpellTypeSettingsLoader : ParentSettingsLoader<SpellTypeSettings, SpellType> { }

    [MessagePackObject]
    public class SpellTypeSettingsMapper : ParentSettingsMapper<SpellTypeSettings, SpellType, SpellTypeSettingsApi> { }
}
