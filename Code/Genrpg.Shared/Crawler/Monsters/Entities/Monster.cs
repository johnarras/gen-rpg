using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Monsters.Entities
{
    [MessagePackObject]
    public class Monster : CrawlerUnit
    {
        [Key(0)] public long MinDam { get; set; }
        [Key(1)] public long MaxDam { get; set; }

        [Key(2)] public List<UnitEffect> Spells { get; set; } = new List<UnitEffect>();
        [Key(3)] public List<UnitEffect> ApplyEffects { get; set; } = new List<UnitEffect>();

        public Monster(IRepositoryService repositoryService) : base(repositoryService) { }
    }
}
