using MessagePack;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Stats.Entities;

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

        [Key(14)] public List<ItemPct> BaseReagents { get; set; }

        public ScalingType()
        {
            AddStats = new List<StatPct>();
            BaseReagents = new List<ItemPct>();
        }
    }
}
