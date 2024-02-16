using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Stats.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    [MessagePackObject]
    public class PartyMember : CrawlerUnit
    {
        [Key(0)] public int PartySlot { get; set; }

        [Key(1)] public long RaceId { get; set; }

        [Key(2)] public List<Item> Equipment { get; set; } = new List<Item>();

        [Key(3)] public List<Stat> PermStats { get; set; } = new List<Stat>();

        [Key(4)] public long Exp { get; set; }

        public long GetPermStat(long statTypeId)
        {
            return PermStats.FirstOrDefault(x => x.Id == statTypeId)?.Val ?? 0;           
        }

        public void SetPermStat(long statTypeId, long val)
        {
            Stat stat = GetPermStatObject(statTypeId);

            stat.Val = (int)val;
        }

        public void AddPermStat(long statTypeId, long val)
        {
            Stat stat = GetPermStatObject(statTypeId);

            stat.Val += (int)val;
        }

        private Stat GetPermStatObject(long statTypeId)
        {
            Stat stat = PermStats.FirstOrDefault(x => x.Id == statTypeId);

            if (stat == null)
            {
                lock (this)
                {
                    stat = PermStats.FirstOrDefault(x => x.Id == statTypeId);

                    if (stat == null)
                    {
                        stat = new Stat() { Id = (short)statTypeId };
                        List<Stat> temp = new List<Stat>(PermStats);
                        temp.Add(stat);
                        PermStats = temp;
                    }
                }
            }

            return stat;
        }

        public override Item GetEquipmentInSlot(long equipSlotId)
        {
            return Equipment.FirstOrDefault(x=>x.EquipSlotId == equipSlotId);
        }

    }
}
