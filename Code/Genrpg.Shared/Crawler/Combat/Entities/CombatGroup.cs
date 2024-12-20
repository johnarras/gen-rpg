using MessagePack;
using System;
using System.Collections.Generic;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    [MessagePackObject]
    public class CombatGroup
    {
        public string Id { get; set; }
        [Key(0)] public List<CrawlerUnit> Units { get; set; } = new List<CrawlerUnit>();
        [Key(1)] public int Range { get; set; }

        [Key(2)] public ECombatGroupActions CombatGroupAction { get; set; }

        [Key(3)] public string SingularName { get; set; }
        [Key(4)] public string PluralName { get; set; }
        [Key(5)] public long UnitTypeId { get; set; }

        public CombatGroup()
        {
            Id = HashUtils.NewGuid();
        }

        public string ShowStatus()
        {
            return Units.Count + " " + (Units.Count == 1 ? SingularName : PluralName) + " (" + Range + "')";
        }

    }
}
