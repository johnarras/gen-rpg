using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Stats.Entities
{
    [MessagePackObject]
    public class ScalingType : IIndexedGameItem
    {

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Prefix { get; set; }
        [Key(5)] public string Art { get; set; }

        [Key(6)] public long CrafterTypeId { get; set; }

        [Key(7)] public int AttPct { get; set; }
        [Key(8)] public int DefPct { get; set; }
        [Key(9)] public int OtherPct { get; set; }

        /// <summary>
        /// Used when calculating buy/sell costs
        /// </summary>
        [Key(10)] public int CostPct { get; set; }

        [Key(11)] public List<StatPct> AddStats { get; set; }

        [Key(12)] public List<ItemPct> BaseReagents { get; set; }

        public ScalingType()
        {
            AddStats = new List<StatPct>();
            BaseReagents = new List<ItemPct>();
        }
    }
}
