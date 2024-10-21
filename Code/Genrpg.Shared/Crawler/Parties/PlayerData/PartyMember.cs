using Genrpg.Shared.Crawler.Items.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{

    [MessagePackObject]
    public class StatRegenFraction
    {
        [Key(0)] public long StatTypeId { get; set; }
        [Key(1)] public double Fraction { get; set; }
    }

    [MessagePackObject]
    public class PartyMember : CrawlerUnit
    {
        [Key(0)] public int PartySlot { get; set; }

        [JsonIgnore]
        [Key(1)] public List<Item> Equipment { get; set; } = new List<Item>();

        [Key(2)] public List<CrawlerSaveItem> SaveEquipment { get; set; } = new List<CrawlerSaveItem>();

        [Key(3)] public string PermStats { get; set; }

        public const long PermStatSize = StatConstants.PrimaryStatEnd - StatConstants.PrimaryStatStart + 1;

        private long[] _permStats { get; set; } = new long[PermStatSize];

        [Key(4)] public List<StatRegenFraction> RegenFractions { get; set; } = new List<StatRegenFraction>();

        public void ClearPermStats()
        {
            _permStats = new long[PermStatSize];
        }

        public void ConvertDataAfterLoad()
        {
            if (!string.IsNullOrEmpty(PermStats))
            {
                string[] words = PermStats.Split(' ');  

                for (int i = 0; i < words.Length && i < _permStats.Length; i++)
                {
                    if (Int64.TryParse(words[i], out long val))
                    {
                        _permStats[i] = val;
                    }
                }
            }
        }

        public void ConvertDataBeforeSave()
        {
            StringBuilder sb = new StringBuilder();
            for (int i =0; i < _permStats.Length; i++)
            {
                sb.Append(_permStats[i].ToString() + " ");
            }
            PermStats = sb.ToString();  
        }


        [Key(5)] public long Exp { get; set; }

        [Key(6)] public List<PartySummon> Summons { get; set; } = new List<PartySummon>();

        [Key(7)] public long WarpMapId { get; set; }
        [Key(8)] public int WarpMapX { get; set; }
        [Key(9)] public int WarpMapZ { get; set; }
        [Key(10)] public int WarpRot { get; set; }
        [Key(11)] public long LastCombatCrawlerSpellId { get; set; }

        public PartyMember(IRepositoryService repositoryService) : base(repositoryService) { }  

        public override bool IsPlayer() { return true; }

        public long GetPermStat(long statTypeId)
        {
            return _permStats[statTypeId-StatConstants.PrimaryStatStart];          
        }

        public void SetPermStat(long statTypeId, long val)
        {
            _permStats[statTypeId-StatConstants.PrimaryStatStart] = val;
        }

        public void AddPermStat(long statTypeId, long val)
        {
            _permStats[statTypeId - StatConstants.PrimaryStatStart] += val;
        }

        public override Item GetEquipmentInSlot(long equipSlotId)
        {
            return Equipment.FirstOrDefault(x=>x.EquipSlotId == equipSlotId);
        }

        protected override bool AlwaysCreateMissingData() { return true; }
    }
}
