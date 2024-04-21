using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Stats.Settings.Scaling
{
    [MessagePackObject]
    public class ScalingType : ChildSettings, IIndexedGameItem
    {


        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Prefix { get; set; }
        [Key(7)] public string Art { get; set; }

        [Key(8)] public long CrafterTypeId { get; set; }

        [Key(9)] public int AttPct { get; set; }
        [Key(10)] public int DefPct { get; set; }
        [Key(11)] public int OtherPct { get; set; }

        /// <summary>
        /// Used when calculating buy/sell costs
        /// </summary>
        [Key(12)] public int CostPct { get; set; }

        [Key(13)] public List<StatPct> AddStats { get; set; }

        [Key(14)] public long BaseItemTypeId { get; set; }
        /// <summary>
        /// Used for Crawler
        /// </summary>
        [Key(15)] public int ArmorPct { get; set; } = 100;

        public ScalingType()
        {
            AddStats = new List<StatPct>();
        }
    }

    /// <summary>
    /// This is used to list required Base reagents for crafting. It's a 
    /// percent so it scales according to the recipe core cost.
    /// </summary>
    [MessagePackObject]
    public class ItemPct
    {
        [Key(0)] public long ItemTypeId { get; set; }
        [Key(1)] public int Percent { get; set; }
    }

    [MessagePackObject]
    public class ScalingTypeSettings : ParentSettings<ScalingType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class ScalingTypeSettingsApi : ParentSettingsApi<ScalingTypeSettings, ScalingType> { }
    [MessagePackObject]
    public class ScalingTypeSettingsLoader : ParentSettingsLoader<ScalingTypeSettings, ScalingType, ScalingTypeSettingsApi> { }

}
