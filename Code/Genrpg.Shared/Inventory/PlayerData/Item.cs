using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Spells.Interfaces;

namespace Genrpg.Shared.Inventory.PlayerData
{
    [MessagePackObject]
    public class Item : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public long ItemTypeId { get; set; }
        [Key(4)] public long Quantity { get; set; }
        [Key(5)] public List<ItemEffect> Effects { get; set; }
        [Key(6)] public long QualityTypeId { get; set; }
        [Key(7)] public long LootRankId { get; set; }
        [Key(8)] public long Level { get; set; }
        [Key(9)] public long ScalingTypeId { get; set; }
        [Key(10)] public long EquipSlotId { get; set; }
        [Key(11)] public long Cost { get; set; }

        [Key(12)] public List<ItemProc> Procs { get; set; } = new List<ItemProc>();

        /// <summary>
        /// Cache this on the item when it's in memory so we don't have to keep looking it up.
        /// </summary>
        /// 
        private string _icon;

        public string GetIcon() { return _icon; }
        public void SetIcon(string icon) { _icon = icon; }

        private string _art;
        public string GetArt() { return _art; }
        public void SetArt(string art) { _art = art; }

        private string _basicInfo;
        public string GetBasicInfo() { return _basicInfo; }
        public void SetBasicInfo(string basicInfo) { _basicInfo = basicInfo; }

        public Item()
        {
            Effects = new List<ItemEffect>();
            Quantity = 1;
        }

    }

    [MessagePackObject]
    public class ItemEffect : IEffect
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long Quantity { get; set; }
        [Key(2)] public long EntityId { get; set; }
    }

}
