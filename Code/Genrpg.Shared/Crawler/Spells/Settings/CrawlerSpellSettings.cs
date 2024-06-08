using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Crawler.Spells.Settings
{
    [MessagePackObject]
    public class CrawlerSpell : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }

        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public int PowerCost { get; set; }
        [Key(8)] public int PowerPerLevel { get; set; }
        [Key(9)] public int MinRange { get; set; } = SpellConstants.MinRange;
        [Key(10)] public int MaxRange { get; set; } = SpellConstants.MaxRange;

        [Key(11)] public long RequiredStatusEffectId { get; set; }
        [Key(12)] public long ReplacesCrawlerSpellId { get; set; }
        [Key(13)] public long CombatActionId { get; set; }
        [Key(14)] public long TargetTypeId { get; set; }
        [Key(15)] public double CritChance { get; set; } = 0;

        [Key(16)] public long Level { get; set; }

        [Key(17)] public List<CrawlerSpellEffect> Effects { get; set; } = new List<CrawlerSpellEffect>();

        [Key(18)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public long GetPowerCost(long level)
        {
            return PowerCost + level * PowerPerLevel;
        }
    }


    [MessagePackObject]
    public class CrawlerSpellEffect
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long MinQuantity { get; set; }
        [Key(3)] public long MaxQuantity { get; set; }
        [Key(4)] public long ElementTypeId { get; set; }

    }

    [MessagePackObject]
    public class CrawlerSpellSettings : ParentSettings<CrawlerSpell>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class CrawlerSpellSettingsApi : ParentSettingsApi<CrawlerSpellSettings, CrawlerSpell> { }
    [MessagePackObject]
    public class CrawlerSpellSettingsLoader : ParentSettingsLoader<CrawlerSpellSettings, CrawlerSpell> { }

    [MessagePackObject]
    public class CrawlerSpellSettingsMapper : ParentSettingsMapper<CrawlerSpellSettings, CrawlerSpell, CrawlerSpellSettingsApi> { }


}
