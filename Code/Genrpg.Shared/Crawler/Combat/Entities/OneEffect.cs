using MessagePack;
using Genrpg.Shared.Inventory.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Spells.Procs.Interfaces;
using Genrpg.Shared.Crawler.Combat.Constants;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    [MessagePackObject]
    public class OneEffect
    {
        [Key(0)] public EHitTypes HitType { get; set; }
        [Key(1)] public long MinQuantity { get; set; }
        [Key(2)] public long MaxQuantity { get; set; }
        [Key(3)] public double CritChance { get; set; }
        [Key(4)] public double PowerPercent { get; set; } = 100;
    }
}
