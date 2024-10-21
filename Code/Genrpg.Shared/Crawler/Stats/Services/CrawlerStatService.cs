using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Stats.Settings;
using Genrpg.Shared.Crawler.Stats.Utils;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Genrpg.Shared.Crawler.Stats.Services
{
    public interface ICrawlerStatService : IInjectable
    {
        void CalcUnitStats(PartyData party, CrawlerUnit unit, bool resetCurrStats);

        void CalcPartyStats(PartyData party, bool resetCurrStats);

        List<NameIdValue> GetInitialStats(PartyMember member);

        List<NameIdValue> GetInitialStatBonuses(long roleId);
    }



    public class CrawlerStatService : ICrawlerStatService
    {
        protected IStatService _statService;
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected IClientRandom _rand;

        public void CalcPartyStats(PartyData party, bool resetCurrStats)
        {
            foreach (PartyMember member in party.Members)
            {
                if (member.PartySlot > 0)
                {
                    CalcUnitStats(party, member, resetCurrStats);
                }
            }
        }

        public void CalcUnitStats(PartyData party, CrawlerUnit unit, bool resetCurrStats)
        {
            if (unit.Level < 1)
            {
                unit.Level = 1;
            }

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(null);

            IReadOnlyList<StatType> allStats = _gameData.Get<StatSettings>(null).GetData();

            IReadOnlyList<Role> allRoles = roleSettings.GetData();

            List<long> buffStatTypes = new List<long>();

            foreach (Role role in allRoles)
            {
                buffStatTypes.AddRange(role.Bonuses.Where(x => x.EntityTypeId == EntityTypes.Stat).Select(x => x.EntityId));
            }

            buffStatTypes = buffStatTypes.Where(x => x < StatConstants.PrimaryStatStart || x > StatConstants.PrimaryStatEnd).ToList();

            buffStatTypes = buffStatTypes.Distinct().ToList();

            List<StatVal> buffStats = GetPartyBuffStats(party);

            List<long> mutableStatTypes = new List<long>() { StatTypes.Health, StatTypes.Mana };

            List<StatVal> currStats = new List<StatVal>();

            if (unit is PartyMember member)
            {
                List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);

                foreach (long mutableStatType in mutableStatTypes)
                {
                    currStats.Add(new StatVal()
                    {
                        StatTypeId = (short)mutableStatType,
                        Val = (int)member.Stats.Curr(mutableStatType),
                    });
                }

                member.Stats.ResetAll();

                for (int primaryStatId = StatConstants.PrimaryStatStart; primaryStatId < StatConstants.PrimaryStatEnd; primaryStatId++)
                {
                    _statService.Add(member, primaryStatId, StatCategories.Base, member.GetPermStat(primaryStatId));
                }


                foreach (long buffStatType in buffStatTypes)
                {

                    long buffVal = buffStats.FirstOrDefault(x => x.StatTypeId == buffStatType)?.Val ?? 0;

                    _statService.Set(member, buffStatType, StatCategories.Bonus, buffVal);
                }

                // Now do equipment.

                foreach (Item item in member.Equipment)
                {
                    foreach (ItemEffect eff in item.Effects)
                    {
                        if (eff.EntityTypeId == EntityTypes.Stat)
                        {
                            _statService.Add(member, eff.EntityId, StatCategories.Bonus, eff.Quantity);
                        }
                    }
                }

                double healthPerLevel = 0;
                double manaPerLevel = 0;

                long stamBonus = CrawlerStatUtils.GetStatBonus(member, StatTypes.Stamina);

                foreach (Role role in roles)
                {
                    healthPerLevel += role.HealthPerLevel;
                    manaPerLevel += role.ManaPerLevel;

                    if (role.ManaStatTypeId > 0)
                    {
                        manaPerLevel += CrawlerStatUtils.GetStatBonus(member, role.ManaStatTypeId);
                    }
                }

                healthPerLevel += stamBonus;

                long totalHealth = (long)(healthPerLevel * unit.Level);
                long totalMana = (long)(manaPerLevel * unit.Level);

                _statService.Set(member, StatTypes.Health, StatCategories.Base, totalHealth);
                _statService.Set(member, StatTypes.Mana, StatCategories.Base, totalMana);

                foreach (long mutableStatType in mutableStatTypes)
                {
                    long currStatVal = currStats.FirstOrDefault(x => x.StatTypeId == mutableStatType).Val;
                    long maxStatVal = member.Stats.Max(mutableStatType);

                    if (resetCurrStats || currStatVal > maxStatVal)
                    {
                        _statService.Set(member, mutableStatType, StatCategories.Curr, maxStatVal);
                    }
                    else
                    {
                        _statService.Set(member, mutableStatType, StatCategories.Curr, currStatVal);
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
                        _statService.Set(unit, statType.IdKey, StatCategories.Base, unit.Level);
                    }
                }
                long minHealth = Math.Max(1, unit.Level / 2);
                long maxHealth = unit.Level * 3 + 10;

                long startHealth = MathUtils.LongRange(minHealth, maxHealth, _rand);

                _statService.Set(unit, StatTypes.Health, StatCategories.Base, startHealth);
                _statService.Set(unit, StatTypes.Health, StatCategories.Curr, startHealth);

                monster.MinDam = 1 + unit.Level / 3;
                monster.MaxDam = 3 + unit.Level / 2;
            }
        }

        private List<StatVal> GetPartyBuffStats(PartyData partyData)
        {
            Dictionary<long, long> buffStatLevels = new Dictionary<long, long>();

            List<StatVal> retval = new List<StatVal>();

            IReadOnlyList<Role> allClasss = _gameData.Get<RoleSettings>(null).GetData();

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(null);

            foreach (PartyMember member in partyData.GetActiveParty())
            {
                List<Role> roles = roleSettings.GetRoles(member.Roles);

                long scalingLevel = member.Level;

                foreach (Role role in roles)
                {
                    List<RoleBonus> buffStatbonuses = role.Bonuses.Where(x => x.EntityTypeId == EntityTypes.Stat).ToList();

                    foreach (RoleBonus bonus in buffStatbonuses)
                    {
                        if (!buffStatLevels.ContainsKey(bonus.EntityId))
                        {
                            buffStatLevels[bonus.EntityId] = 0;
                        }

                        if (buffStatLevels[bonus.EntityId] < scalingLevel)
                        {
                            buffStatLevels[bonus.EntityId] = scalingLevel;
                        }
                    }
                }
            }

            if (partyData.Combat != null)
            {
                foreach (StatVal combatBuff in partyData.Combat.StatBuffs)
                {
                    if (!buffStatLevels.ContainsKey(combatBuff.StatTypeId))
                    {
                        buffStatLevels[combatBuff.StatTypeId] = 0;
                    }
                    buffStatLevels[combatBuff.StatTypeId] += combatBuff.Val;
                }
            }


            foreach (long statId in buffStatLevels.Keys)
            {
                retval.Add(new StatVal() { StatTypeId = (short)statId, Val = (int)buffStatLevels[statId] });
            }

            return retval;
        }


        public List<NameIdValue> GetInitialStats(PartyMember member)
        {

            CrawlerStatSettings crawlerStatSettings = _gameData.Get<CrawlerStatSettings>(_gs.ch);

            IReadOnlyList<StatType> allStats = _gameData.Get<StatSettings>(null).GetData().Where(x => x.IdKey >=
            StatConstants.PrimaryStatStart && x.IdKey <= StatConstants.PrimaryStatEnd).OrderBy(x => x.IdKey).ToList();

            List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);


            List<NameIdValue> retval = new List<NameIdValue>();

            foreach (StatType statType in allStats)
            {
                retval.Add(new NameIdValue()
                {
                    IdKey = statType.IdKey,
                    Name = statType.Name,
                    Val = crawlerStatSettings.StartStat,
                });
            }

            foreach (Role role in roles)
            {
                List<NameIdValue> nameVals = GetInitialStatBonuses(role.IdKey);

                foreach (NameIdValue nameVal in nameVals)
                {
                    NameIdValue currStatVal = retval.FirstOrDefault(x => x.IdKey== nameVal.IdKey);

                    if (currStatVal != null)
                    {
                        currStatVal.Val += nameVal.Val;
                    }
                }
            }

            return retval;
        }

        public List<NameIdValue> GetInitialStatBonuses(long roleId)
        {
            IReadOnlyList<StatType> allStats = _gameData.Get<StatSettings>(null).GetData().Where(x => x.IdKey >=
            StatConstants.PrimaryStatStart && x.IdKey <= StatConstants.PrimaryStatEnd).OrderBy(x => x.IdKey).ToList();

            Role role = _gameData.Get<RoleSettings>(_gs.ch).Get(roleId);

            List<NameIdValue> retval = new List<NameIdValue>();

            if (String.IsNullOrEmpty(role.StartStatBonuses))
            {
                return retval;
            }

            string[] words = role.StartStatBonuses.Split(' ');

            for (int i = 0; i < allStats.Count && i < words.Length; i++)
            {
                if (Int64.TryParse(words[i], out long val))
                {
                    if (val != 0)
                    {
                        retval.Add(new NameIdValue()
                        {
                            IdKey = allStats[i].IdKey,
                            Name = allStats[i].Name.Substring(0,3),
                            Val = val
                        });
                    }
                }
            }

            return retval;

        }
    }
}
