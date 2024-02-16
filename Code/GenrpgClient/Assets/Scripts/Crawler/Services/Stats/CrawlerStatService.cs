using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using System.Linq;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System.Security.Cryptography.X509Certificates;
using Genrpg.Shared.Crawler.Parties.Constants;
using Genrpg.Shared.Crawler.Stats.Utils;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Entities.Constants;

namespace Genrpg.Shared.Crawler.Stats.Services
{
    public class CrawlerStatService : ICrawlerStatService
    {
        IStatService _statService;

        public async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }


        public void CalcPartyStats(GameState gs, PartyData party, bool resetCurrStats)
        {
            foreach (PartyMember member in party.Members)
            {
                if (member.PartySlot > 0)
                {
                    CalcUnitStats(gs, party, member, resetCurrStats);
                }
            }
        }

        public void CalcUnitStats(GameState gs, PartyData party, CrawlerUnit unit, bool resetCurrStats)
        {
            if (unit.Level < 1)
            {
                unit.Level = 1;
            }

            IReadOnlyList<StatType> allStats = gs.data.Get<StatSettings>(null).GetData();

            IReadOnlyList<Class> allClasses = gs.data.Get<ClassSettings>(null).GetData();

            List<long> buffStatTypes = allClasses.Select(x=>x.BuffStatTypeId).ToList();

            List<Stat> buffStats = GetPartyBuffStats(gs, party);

            List<long> mutableStatTypes = new List<long>() { StatTypes.Health, StatTypes.Mana };

            List<Stat> currStats = new List<Stat>();

            if (unit is PartyMember member)
            {
                List<Class> memberClasses = gs.data.Get<ClassSettings>(null).GetClasses(member.Classes);

                List<Stat> permStats = member.PermStats;

                foreach (long mutableStatType in mutableStatTypes)
                {
                    currStats.Add(new Stat()
                    {
                        Id = (short)mutableStatType,
                        Val = (int)member.Stats.Curr(mutableStatType),
                    });
                }

                member.Stats.ResetAll();

                foreach (Stat permStat in permStats)
                {
                    _statService.Add(gs, member, permStat.Id, StatCategories.Base, permStat.Val);
                }

                // Now set the default buffs

                foreach (Class arch in allClasses)
                {
                    if (arch.BuffStatTypeId > 0 && arch.DefaultLevelPercentBuff > 0)
                    {
                        _statService.Add(gs, member, arch.BuffStatTypeId, StatCategories.Bonus,
                            (unit.Level * arch.DefaultLevelPercentBuff) / 100);
                    }
                }

                foreach (long buffStatType in buffStatTypes)
                {

                    long buffVal = buffStats.FirstOrDefault(x => x.Id == buffStatType)?.Val ?? 0;

                    _statService.Set(gs, member, buffStatType, StatCategories.Bonus, buffVal);
                }

                // Now do equipment.

                foreach (Item item in member.Equipment)
                {
                    foreach (ItemEffect eff in item.Effects)
                    {
                        if (eff.EntityTypeId == EntityTypes.Stat)
                        {
                            _statService.Add(gs, member, eff.EntityId, StatCategories.Bonus, eff.Quantity);
                        }
                    }
                }

                long healthPerLevel = 0;
                long manaPerLevel = 0;

                long stamBonus = CrawlerStatUtils.GetStatBonus(member,StatTypes.Stamina);
                long intbonus = CrawlerStatUtils.GetStatBonus(member, StatTypes.Intellect);
                foreach (Class cl in memberClasses)
                {
                    healthPerLevel += cl.HealthPerLevel;
                    manaPerLevel += cl.ManaPerLevel;
                }

                healthPerLevel += stamBonus;
                if (manaPerLevel > 0)
                {
                    manaPerLevel += intbonus;
                }

                long totalHealth = (healthPerLevel * unit.Level);
                long totalMana = (manaPerLevel * unit.Level);

                _statService.Set(gs, member, StatTypes.Health, StatCategories.Base, totalHealth);
                _statService.Set(gs, member, StatTypes.Mana, StatCategories.Base, totalMana);

                foreach (long mutableStatType in mutableStatTypes)
                {
                    long currStatVal = currStats.FirstOrDefault(x => x.Id == mutableStatType).Val;
                    long maxStatVal = member.Stats.Max(mutableStatType);

                    if (resetCurrStats || currStatVal > maxStatVal)
                    {
                        _statService.Set(gs, member, mutableStatType, StatCategories.Curr, maxStatVal);
                    }
                }
            }
            else if (unit is Monster monster)
            {
                foreach (StatType statType in allStats)
                {
                    if ((statType.IdKey >= StatConstants.PrimaryStatStart && statType.IdKey <= StatConstants.PrimaryStatEnd) ||
                        buffStatTypes.Contains(statType.IdKey))
                    {
                        _statService.Set(gs, unit, statType.IdKey, StatCategories.Base, unit.Level);
                    }
                }
                long minHealth = Math.Max(1, unit.Level / 2);
                long maxHealth = unit.Level * 3 + 10;

                long startHealth = MathUtils.LongRange(minHealth, maxHealth, gs.rand);

                _statService.Set(gs, unit, StatTypes.Health, StatCategories.Base, startHealth);
                _statService.Set(gs, unit, StatTypes.Health, StatCategories.Curr, startHealth);

                monster.MinDam = 1 + unit.Level / 5;
                monster.MaxDam = 2 + unit.Level / 2;
            }
        }

        public List<Stat> GetPartyBuffStats(GameState gs, PartyData partyData)
        {
            Dictionary<long, long> buffStatLevels = new Dictionary<long, long>();

            List<Stat> retval = new List<Stat>();

            IReadOnlyList<Class> allClasss = gs.data.Get<ClassSettings>(null).GetData();

            ClassSettings classSettings = gs.data.Get<ClassSettings>(null);

            foreach (PartyMember member in partyData.GetActiveParty())
            {

                List<Class> memberClasses = classSettings.GetClasses(member.Classes);

                foreach (Class cl in memberClasses)
                {
                    if (!buffStatLevels.ContainsKey(cl.BuffStatTypeId))
                    {
                        buffStatLevels[cl.BuffStatTypeId] = 0;
                    }
                    
                    if (buffStatLevels[cl.BuffStatTypeId] < member.Level)
                    {
                        buffStatLevels[cl.BuffStatTypeId] = member.Level;
                    }
                }
            }

            foreach (long statId in buffStatLevels.Keys)
            {
                Class arch = allClasss.FirstOrDefault(x => x.BuffStatTypeId == statId);

                // Apply a percent bonus from each archetype (50 to start) and divide by 2 since each player
                // has 2 archetypes.
                int scaledStatValue = (int)((buffStatLevels[statId] * arch.BuffStatPercent) / 100);

                retval.Add(new Stat() { Id = (short)statId, Val = scaledStatValue });
            }

            return retval;
        }
    }
}
