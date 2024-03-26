using MessagePack;
using Genrpg.Shared.Crawler.Monsters.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Crawler.Spells.Entities
{
    /// <summary>
    /// Contains data about what this unit will do this round during combat.
    /// </summary>
    [MessagePackObject]
    public class UnitAction
    {

        [Key(0)] public string Text { get; set; }

        [Key(1)] public CrawlerUnit Caster { get; set; }

        [Key(2)] public List<CrawlerUnit> PossibleTargetUnits { get; set; } = new List<CrawlerUnit>();

        [Key(3)] public List<CombatGroup> PossibleTargetGroups { get; set; } = new List<CombatGroup>();

        [Key(4)] public List<CrawlerUnit> FinalTargets { get; set; } = new List<CrawlerUnit>();

        [Key(5)] public List<CombatGroup> FinalTargetGroups { get; set; } = new List<CombatGroup>();

        [Key(6)] public long CombatActionId { get; set; }

        [Key(7)] public CrawlerSpell Spell { get; set; }

        [Key(8)] public bool IsComplete { get; set; }

        [Key(9)] public Item CastingItem { get; set; }
    }
}
