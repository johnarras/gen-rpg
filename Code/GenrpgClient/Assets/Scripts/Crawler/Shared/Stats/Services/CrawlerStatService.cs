using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Monsters.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roguelikes.Constants;
using Genrpg.Shared.Crawler.Roguelikes.Services;
using Genrpg.Shared.Crawler.Roguelikes.Settings;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers;
using Genrpg.Shared.Crawler.Stats.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Stats.Services
{
    public interface ICrawlerStatService : IInjectable
    {
        void CalcUnitStats(PartyData party, CrawlerUnit unit, bool resetCurrStats);

        void CalcPartyStats(PartyData party, bool resetCurrStats);

        List<NameIdValue> GetInitialStats(PartyData party, PartyMember member);

        List<NameIdValue> GetInitialStatBonuses(long roleId);

        long GetStatBonus(PartyData party, CrawlerUnit unit, long statId);


        void Add(PartyData party, CrawlerUnit unit, long statTypeId, int statCategory, long value, long elementTypeId = 0);
    }



    public class CrawlerStatService : ICrawlerStatService
    {
        protected IStatService _statService;
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected IClientRandom _rand;
        private IRoguelikeUpgradeService _roguelikeUpgradeService;
        private IDispatcher _dispatcher;

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

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);
            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            CrawlerMonsterSettings monsterSettings = _gameData.Get<CrawlerMonsterSettings>(_gs.ch);
            CrawlerStatSettings statSettings = _gameData.Get<CrawlerStatSettings>(_gs.ch);

            IReadOnlyList<StatType> allStats = _gameData.Get<StatSettings>(_gs.ch).GetData();

            IReadOnlyList<Role> allRoles = roleSettings.GetData();

            List<long> buffStatTypes = new List<long>();

            foreach (Role role in allRoles)
            {
                buffStatTypes.AddRange(role.BinaryBonuses.Where(x => x.EntityTypeId == EntityTypes.Stat).Select(x => x.EntityId));
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

                    long buffVal = member.Level; // buffStats.FirstOrDefault(x => x.StatTypeId == buffStatType)?.Val ?? 0;

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

                long stamBonus = GetStatBonus(party, member, StatTypes.Stamina);

                foreach (Role role in roles)
                {
                    healthPerLevel += role.HealthPerLevel;
                    manaPerLevel += role.ManaPerLevel;

                    if (role.ManaStatTypeId > 0)
                    {
                        manaPerLevel += GetStatBonus(party, member, role.ManaStatTypeId);
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
                UnitType unitType = _gameData.Get<UnitSettings>(_gs.ch).Get(unit.UnitTypeId);

                List<UnitEffect> effects = unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.Stat).ToList();

                foreach (UnitEffect effect in effects)
                {
                    _statService.Set(unit, effect.EntityId, StatCategories.Bonus, effect.Quantity);
                }

                foreach (StatType statType in allStats)
                {
                    if (statType.IdKey >= StatConstants.PrimaryStatStart && statType.IdKey <= StatConstants.PrimaryStatEnd)
                    {
                        _statService.Set(unit, statType.IdKey, StatCategories.Base, unit.Level + statSettings.StartStat);
                    }
                    else if (buffStatTypes.Contains(statType.IdKey))
                    {
                        _statService.Set(unit, statType.IdKey, StatCategories.Base, unit.Level);
                    }

                }

                long minHealth = (long)(monsterSettings.BaseMinHealth + unit.Level * monsterSettings.MinHealthPerLevel);
                long maxHealth = (long)(monsterSettings.BaseMaxHealth + unit.Level * monsterSettings.MaxHealthPerLevel);

                monster.MinDam = (long)(monsterSettings.BaseMinDam + unit.Level * monsterSettings.MinDamPerLevel);
                monster.MaxDam = (long)(monsterSettings.BaseMaxDam + unit.Level * monsterSettings.MaxDamPerLevel);

                double qualityScaling = _roguelikeUpgradeService.GetBonus(party, RoguelikeUpgrades.SummonQuality);

                double levelHealthScaling = 0;
                double levelDamageScaling = 0;

                if (unit.FactionTypeId != FactionTypes.Player)
                {
                    levelHealthScaling = monsterSettings.ExtraHealthScalePerLevel * unit.Level;
                    levelDamageScaling = monsterSettings.ExtraDamageScalePerLevel * unit.Level;
                }

                minHealth = (long)(minHealth * (1 + qualityScaling + levelHealthScaling));
                maxHealth = (long)(maxHealth * (1 + qualityScaling + levelHealthScaling));
                monster.MinDam = (long)(monster.MinDam * (1 + qualityScaling + levelDamageScaling));
                monster.MaxDam = (long)(monster.MaxDam * (1 + qualityScaling + levelDamageScaling));
                
                long startHealth = MathUtils.LongRange(minHealth, maxHealth, _rand);


                _statService.Set(unit, StatTypes.Health, StatCategories.Base, startHealth);
                _statService.Set(unit, StatTypes.Health, StatCategories.Curr, startHealth);

                long maxMana = unit.Level * monsterSettings.ManaPerLevel;

                _statService.Set(unit, StatTypes.Mana, StatCategories.Base, maxMana);
                _statService.Set(unit, StatTypes.Mana, StatCategories.Curr, maxMana);

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
                    List<RoleBonusBinary> buffStatbonuses = role.BinaryBonuses.Where(x => x.EntityTypeId == EntityTypes.Stat).ToList();

                    foreach (RoleBonusBinary bonus in buffStatbonuses)
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


        public List<NameIdValue> GetInitialStats(PartyData party, PartyMember member)
        {

            CrawlerStatSettings crawlerStatSettings = _gameData.Get<CrawlerStatSettings>(_gs.ch);

            IReadOnlyList<StatType> allStats = _gameData.Get<StatSettings>(null).GetData().Where(x => x.IdKey >=
            StatConstants.PrimaryStatStart && x.IdKey <= StatConstants.PrimaryStatEnd).OrderBy(x => x.IdKey).ToList();
            List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);


            long roguelikeBonusStats = 0;


            if (party.GameMode == ECrawlerGameModes.Roguelite)
            {
                roguelikeBonusStats = (long)_roguelikeUpgradeService.GetBonus(party, RoguelikeUpgrades.StartStats);
            }
            List<NameIdValue> retval = new List<NameIdValue>();

            foreach (StatType statType in allStats)
            {
                retval.Add(new NameIdValue()
                {
                    IdKey = statType.IdKey,
                    Name = statType.Name,
                    Val = crawlerStatSettings.StartStat + roguelikeBonusStats,
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

        private EGameModes EGameModes(long v)
        {
            throw new NotImplementedException();
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

        public long GetStatBonus(PartyData party, CrawlerUnit unit, long statTypeId)
        {
            if (statTypeId < 1)
            {
                return 0;
            }

            long statValue = unit.Stats.Max(statTypeId);

            long baseBonus = 0;

            if (statValue < 13)
            {
                baseBonus = 0;
            }
            else if (statValue < 16)
            {
                baseBonus = 1;
            }
            else if (statValue < 18)
            {
                baseBonus = 2;
            }
            else if (statValue <= 20) // 18-20 go from 3 to 5
            {
                baseBonus = (statValue - 18) + 3;
            }
            else if (statValue <= 50) // 21-50 go from 6 to 11
            {
                baseBonus = 5 + (statValue - 20) / 5;
            }
            else // 11 at 50 then 1 point per 10 stat vals after. Cap at 250 for 11 + 11 = 22
            {
                if (statValue > 250)
                {
                    statValue = 250;
                }

                baseBonus = 11 + (statValue - 50) / 10;
            }

            if (party.GameMode == ECrawlerGameModes.Roguelite)
            {
                baseBonus += (long)_roguelikeUpgradeService.GetBonus(party, RoguelikeUpgrades.StatBonusValue);
            }

            return baseBonus;

        }

        public void Add(PartyData party, CrawlerUnit unit, long statTypeId, int statCategory, long value, long elementTypeId = 0)
        {
            _statService.Add(unit, statTypeId, statCategory, value);
            _dispatcher.Dispatch(new RefreshUnitStatus() { Unit = unit, ElementTypeId = elementTypeId });
        }
    }
}
