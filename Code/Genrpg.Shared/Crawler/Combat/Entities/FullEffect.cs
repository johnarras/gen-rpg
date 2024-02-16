using MessagePack;
using Genrpg.Shared.Crawler.Spells.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Spells.Settings.Elements;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    [MessagePackObject]
    public class FullEffect
    {
        [Key(0)] public CrawlerSpellEffect Effect { get; set; }
        [Key(1)] public OneEffect Hit { get; set; }
        [Key(2)] public ElementType ElementType { get; set; }
        [Key(3)] public double PercentChance { get; set; } = 100;
        [Key(4)] public bool InitialEffect { get; set; }

    }
}
