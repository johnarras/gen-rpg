using System;
using System.Collections.Generic;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using System.Linq;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Crawler.Stats.Utils;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Assets.Scripts.ProcGen.RandomNumbers;

namespace Genrpg.Shared.Crawler.Stats.Services
{
    public class CrawlerStatService : ICrawlerStatService
    {
        protected IStatService _statService;
        protected IGameData _gameData;
        protected IUnityGameState _gs;
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

            buffStatTypes = buffStatTypes.Distinct().ToList();
            
            List<MemberStat> buffStats = GetPartyBuffStats(party);

            List<long> mutableStatTypes = new List<long>() { StatTypes.Health, StatTypes.Mana };

            List<MemberStat> currStats = new List<MemberStat>();

            if (unit is PartyMember member)
            {
                List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);

                List<MemberStat> permStats = member.PermStats;

                foreach (long mutableStatType in mutableStatTypes)
                {
                    currStats.Add(new MemberStat()
                    {
                        Id = (short)mutableStatType,
                        Val = (int)member.Stats.Curr(mutableStatType),
                    });
                }

                member.Stats.ResetAll();

                foreach (MemberStat permStat in permStats)
                {
                    _statService.Add(member, permStat.Id, StatCategories.Base, permStat.Val);
                }


                foreach (long buffStatType in buffStatTypes)
                {

                    long buffVal = buffStats.FirstOrDefault(x => x.Id == buffStatType)?.Val ?? 0;

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

                long stamBonus = CrawlerStatUtils.GetStatBonus(member,StatTypes.Stamina);

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
                    long currStatVal = currStats.FirstOrDefault(x => x.Id == mutableStatType).Val;
                    long maxStatVal = member.Stats.Max(mutableStatType);

                    if (resetCurrStats || currStatVal > maxStatVal)
                    {
                        _statService.Set(member, mutableStatType, StatCategories.Curr, maxStatVal);
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

                monster.MinDam = 1 + unit.Level / 5;
                monster.MaxDam = 2 + unit.Level / 2;
            }
        }

        public List<MemberStat> GetPartyBuffStats(PartyData partyData)
        {
            Dictionary<long, long> buffStatLevels = new Dictionary<long, long>();

            List<MemberStat> retval = new List<MemberStat>();

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

            foreach (long statId in buffStatLevels.Keys)
            {
                retval.Add(new MemberStat() { Id = (short)statId, Val = (int)buffStatLevels[statId] });
            }

            return retval;
        }
    }
}
