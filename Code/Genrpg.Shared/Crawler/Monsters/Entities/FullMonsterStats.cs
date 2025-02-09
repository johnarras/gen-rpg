using MessagePack;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Monsters.Entities
{
    [MessagePackObject]
    public class FullMonsterStats
    {
        [Key(0)] public List<UnitEffect> Spells { get; set; } = new List<UnitEffect>();
        [Key(1)] public List<FullEffect> ApplyEffects { get; set; } = new List<FullEffect>();
        [Key(2)] public bool IsGuardian { get; set; }
        [Key(3)] public long ResistBits { get; set; }
        [Key(4)] public long VulnBits { get; set; }
        [Key(5)] public long Range { get; set; } = CrawlerCombatConstants.MinRange;
    }
}
