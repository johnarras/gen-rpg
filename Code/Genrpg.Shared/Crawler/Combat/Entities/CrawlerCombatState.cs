using MessagePack;
using Genrpg.Shared.Crawler.Monsters.Entities;
using System;
using System.Collections.Generic;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;

namespace Genrpg.Shared.Crawler.Combat.Entities
{

    [MessagePackObject]
    public class InitialCombatState
    {
        [Key(0)] public long Level { get; set; }
        [Key(1)] public double Difficulty { get; set; } = 1.0f;
        [Key(2)] public List<InitialCombatGroup> CombatGroups { get; set; } = new List<InitialCombatGroup>();
    }


    [MessagePackObject]
    public class InitialCombatGroup
    {
        [Key(0)] public long UnitTypeId { get; set; }
        [Key(1)] public long Quantity { get; set; }
        [Key(2)] public int Range { get; set; }
    }


    [MessagePackObject]
    public class CrawlerCombatState
    {
        [Key(0)] public int RoundsComplete { get; set; } = 0;

        [Key(1)] public long Level { get; set; } = 1;

        [Key(2)] public List<CombatGroup> Enemies { get; set; } = new List<CombatGroup>();

        [Key(3)] public List<CombatGroup> Allies { get; set; } = new List<CombatGroup>();
       
        [Key(4)] public List<CrawlerUnit> EnemiesKilled { get; set; } = new List<CrawlerUnit>();

        [Key(5)] public CombatGroup PartyGroup { get; set; }

        [Key(6)] public List<StatVal> StatBuffs { get; set; } = new List<StatVal>();

        [Key(7)] public long PlayerActionsRemaining { get; set; }



        public bool PartyWonCombat() { return Enemies.Count == 0; }

        public bool PartyIsDead()
        {
            bool haveAlivePartyMember = false;

            foreach (CombatGroup group in Allies)
            {
                foreach (CrawlerUnit unit in group.Units)
                {
                    if (unit is PartyMember member &&
                        !member.StatusEffects.HasBit(StatusEffects.Dead))
                    {
                        haveAlivePartyMember = true;
                        break;
                    }
                }
            }

            return !haveAlivePartyMember;
        }

        public List<CrawlerUnit> GetAllUnits()
        {
            List<CrawlerUnit> allUnits = new List<CrawlerUnit>();

            foreach (CombatGroup group in Allies)
            {
                allUnits.AddRange(group.Units);
            }

            foreach (CombatGroup group in Enemies)
            {
                allUnits.AddRange(group.Units);
            }

            return allUnits;
        }
    }
}
