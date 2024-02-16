using MessagePack;
using Genrpg.Shared.Crawler.Monsters.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    [MessagePackObject]
    public class CombatUpdate
    {
        [Key(0)] public CrawlerUnit Attacker { get; set; }
        [Key(1)] public CrawlerUnit Defender { get; set; }
        [Key(2)] public long EntityTypeId { get; set; }
        [Key(3)] public long EntityId { get; set; }
        [Key(4)] public long Quantity { get; set; }
    }
}
