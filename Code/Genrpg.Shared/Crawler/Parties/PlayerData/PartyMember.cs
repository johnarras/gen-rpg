using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
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

        [Key(3)] public List<MemberStat> PermStats { get; set; } = new List<MemberStat>();

        [Key(4)] public long Exp { get; set; }

        [Key(5)] public List<PartySummon> Summons { get; set; } = new List<PartySummon>();

        public PartyMember(IRepositoryService repositoryService) : base(repositoryService) { }  

        public override bool IsPlayer() { return true; }

        public long GetPermStat(long statTypeId)
        {
            return PermStats.FirstOrDefault(x => x.Id == statTypeId)?.Val ?? 0;           
        }

        public void SetPermStat(long statTypeId, long val)
        {
            MemberStat stat = GetPermStatObject(statTypeId);

            stat.Val = (int)val;
        }

        public void AddPermStat(long statTypeId, long val)
        {
            MemberStat stat = GetPermStatObject(statTypeId);

            stat.Val += (int)val;
        }

        private MemberStat GetPermStatObject(long statTypeId)
        {
            MemberStat stat = PermStats.FirstOrDefault(x => x.Id == statTypeId);

            if (stat == null)
            {
                lock (this)
                {
                    stat = PermStats.FirstOrDefault(x => x.Id == statTypeId);

                    if (stat == null)
                    {
                        stat = new MemberStat() { Id = (short)statTypeId };
                        List<MemberStat> temp = new List<MemberStat>(PermStats);
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

    [MessagePackObject]
    public class MemberStat
    {
        [Key(0)] public short Id { get; set; }
        [Key(1)] public int Val { get; set; }
    }

}
