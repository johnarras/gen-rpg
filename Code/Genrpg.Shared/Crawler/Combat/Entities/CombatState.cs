using MessagePack;
using Genrpg.Shared.Crawler.Monsters.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Crawler.Combat.Utils;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Crawler.UI.Interfaces;
using System.Net.Http.Headers;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    [MessagePackObject]
    public class CombatState
    {
        [Key(0)] public int Round { get; set; } = 0;

        [Key(1)] public long Level { get; set; } = 1;

        [Key(2)] public List<CombatGroup> Enemies { get; set; } = new List<CombatGroup>();

        [Key(3)] public List<CombatGroup> Allies { get; set; } = new List<CombatGroup>();
       
        [Key(4)] public List<CrawlerUnit> EnemiesKilled { get; set; } = new List<CrawlerUnit>();

        [Key(5)] public CombatGroup PartyGroup { get; set; }

        public bool CombatIsOver() { return Enemies.Count == 0 || PartyIsDead(); }

        public bool PartyIsDead()
        {
            bool haveAlivePartyMember = false;

            foreach (CombatGroup group in Allies)
            {
                foreach (CrawlerUnit unit in group.Units)
                {
                    if (!unit.StatusEffects.HasBit(StatusEffects.Dead))
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
