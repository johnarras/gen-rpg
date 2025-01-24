using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Newtonsoft.Json.Serialization;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Spells.PlayerData.Spells;
using System.Reflection.Emit;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Roguelikes.Settings;
using Genrpg.Shared.Crawler.Monsters.Settings;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Factions.Settings;

namespace Genrpg.Shared.Crawler.Combat.Services
{
    public interface ICrawlerCombatService : IInitializable
    {
        void CheckForEncounter(bool atEndOfMove, CancellationToken token);
        Task<bool> StartCombat(PartyData partyData, InitialCombatState initialState = null);
        Task EndCombatRound(PartyData party);
        bool SetMonsterActions(PartyData party);
        bool ReadyForCombat(PartyData party);
        bool IsDisabled(CrawlerUnit unit);
        bool IsActionBlocked(PartyData party, CrawlerUnit unit, long combatActionId);
        bool IsActionWeak(CrawlerUnit unit, long combatActionId);
        List<UnitAction> GetActionsForPlayer(PartyData party, CrawlerUnit unit);
        UnitAction GetActionFromSpell(PartyData party, CrawlerUnit unit, CrawlerSpell spell,
            List<UnitAction> currentActions = null);
        void SetInitialActions(PartyData party);
        void AddCombatUnits(PartyData partyData, UnitType unitType, long unitQuantity, long factionTypeId,
            int currRange = CrawlerCombatConstants.MinRange);

        FullMonsterStats GetFullMonsterStats(UnitType unitType, long combatLevel);
    }
    public class CrawlerCombatService : ICrawlerCombatService
    {
        private ICrawlerStatService _statService = null;
        private ICrawlerSpellService _spellService = null;
        protected IGameData _gameData = null;
        private IRepositoryService _repoService = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        private ICrawlerMapService _crawlerMapService = null;
        private ICrawlerService _crawlerService = null;
        private ICrawlerWorldService _worldService = null;
        private ILogService _logService = null;
        private ITimeOfDayService _timeService = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<bool> StartCombat(PartyData party, InitialCombatState initialState = null)
        {

            if (initialState == null)
            {
                return false;
            }

            if (party.Combat != null)
            {
                return false;
            }
            party.NextGroupId = 0;

            if (initialState == null)
            {
                initialState = new InitialCombatState()
                {
                };
            }

            if (initialState.Level < 1)
            {
                initialState.Level = await _worldService.GetMapLevelAtParty
                (await _worldService.GetWorld(party.WorldId), party);
            }

            party.Combat = new CrawlerCombatState() { Level = initialState.Level };

            List<PartyMember> members = party.GetActiveParty();

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            RoguelikeSettings roguelikeSettings = _gameData.Get<RoguelikeSettings>(_gs.ch);

            foreach (PartyMember member in members)
            {

                if (member.Summons.Count > 0)
                {
                    long quantity = _spellService.GetSummonQuantity(party, member);

                    foreach (PartySummon summon in member.Summons)
                    {
                        UnitType unitType = _gameData.Get<UnitSettings>(_gs.ch).Get(summon.UnitTypeId);

                        if (unitType != null)
                        {
                            AddCombatUnits(party, unitType, Math.Max(1,(long)Math.Max(1,quantity*unitType.SpawnQuantityScale)), FactionTypes.Player);
                        }
                    }
                }
            }

            CombatGroup partyGroup = new CombatGroup() { SingularName = "Player", PluralName = "Players", Id=party.GetNextGroupId() };
            party.Combat.Allies.Add(partyGroup);
            party.Combat.PartyGroup = partyGroup;

            IReadOnlyList<UnitType> allUnitTypes = _gameData.Get<UnitSettings>(null).GetData();

            foreach (PartyMember member in members)
            {
                partyGroup.Units.Add(member);
                member.CombatGroupId = partyGroup.Id;
            }

            if (initialState.CombatGroups.Count < 1)
            {
                ZoneType zoneType = await _worldService.GetCurrentZone(party);

                List<ZoneUnitSpawn> spawns = new List<ZoneUnitSpawn>();

                if (zoneType != null && zoneType.ZoneUnitSpawns.Count > 0)
                {
                    spawns = new List<ZoneUnitSpawn>(zoneType.ZoneUnitSpawns);
                }
                else
                {
                    foreach (UnitType utype in allUnitTypes)
                    {
                        if (utype.IdKey > 0)
                        {
                            spawns.Add(new ZoneUnitSpawn() { UnitTypeId = utype.IdKey, Chance = 1 });
                        }
                    }
                }

                double difficulty = Math.Max(0.5f, initialState.Difficulty);

                int groupCount = 1;

                groupCount += MathUtils.IntRange(0, (int)initialState.Level / 30, _rand);

                while (_rand.NextDouble() < 0.1f * difficulty && groupCount < CrawlerCombatConstants.MaxStartEnemyGroupCount)
                {
                    groupCount++;
                }

                int maxGroups = (int)Math.Max(1, (party.Combat.Level / 5.0f + 1) * difficulty);

                if (_rand.NextDouble() < 0.2f)
                {
                    maxGroups++;
                }

                groupCount = MathUtils.Clamp(1, groupCount, maxGroups);
                groupCount = 2;
                List<UnitType> chosenUnitTypes = new List<UnitType>();

                while (chosenUnitTypes.Count < groupCount && spawns.Count > 0)
                {
                    double chanceSum = spawns.Sum(x => x.Chance);

                    double chanceChosen = _rand.NextDouble() * chanceSum;

                    foreach (ZoneUnitSpawn sp in spawns)
                    {
                        chanceChosen -= sp.Chance;
                        if (chanceChosen <= 0)
                        {
                            UnitType newUnitType = allUnitTypes.FirstOrDefault(x => x.IdKey == sp.UnitTypeId);
                            if (newUnitType != null)
                            {
                                chosenUnitTypes.Add(newUnitType);
                            }
                            spawns.Remove(sp);
                            break;
                        }
                    }
                }

                int currRange = CrawlerCombatConstants.MinRange;

                foreach (UnitType unitType in chosenUnitTypes)
                {

                    int quantity = (int)(MathUtils.IntRange(1, 10, _rand) * difficulty);

                    while (_rand.NextDouble() < 0.1f * difficulty && quantity < 99)
                    {
                        quantity += MathUtils.IntRange(1, 10, _rand);
                    }

                    if (party.Combat.Level >= 10)
                    {
                        while (_rand.NextDouble() < 0.1f * difficulty && quantity < 99)
                        {
                            quantity += MathUtils.IntRange(2, 20, _rand);
                        }
                    }

                    if (_gs.GameMode == EGameModes.Roguelike)
                    {
                        quantity = Math.Max(1, (int)(quantity*roguelikeSettings.MonsterQuantityScale));  
                    }

                    quantity = MathUtils.Clamp(1, quantity, 99);

                    InitialCombatGroup initialGroup = new InitialCombatGroup()
                    {
                        UnitTypeId = unitType.IdKey,
                        Range = currRange,
                        Quantity = quantity,
                    };

                    initialState.CombatGroups.Add(initialGroup);

                    if (_rand.NextDouble() < 0.6f)
                    {
                        currRange += CrawlerCombatConstants.RangeDelta;

                        if (_rand.NextDouble() < 0.2f)
                        {
                            currRange += CrawlerCombatConstants.RangeDelta;
                        }
                    }
                }
            }

            foreach (InitialCombatGroup initialGroup in initialState.CombatGroups)
            {
                UnitType unitType = allUnitTypes.FirstOrDefault(x => x.IdKey == initialGroup.UnitTypeId);
                AddCombatUnits(party, unitType, initialGroup.Quantity, FactionTypes.Faction1, initialGroup.Range);
            }

            party.WorldPanel.UpdateCombatGroups();
            return true;
        }

        public FullMonsterStats GetFullMonsterStats(UnitType unitType, long combatLevel)
        {
            FullMonsterStats retval = new FullMonsterStats();

            IReadOnlyList<CrawlerSpell> crawlerSpells = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData();

            List<long> defendSpellIds = crawlerSpells.Where(x => x.CombatActionId == CombatActions.Defend).Select(x => x.IdKey).ToList();

            IReadOnlyList<StatusEffect> statusEffects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);

            ElementTypeSettings elementSettings = _gameData.Get<ElementTypeSettings>(_gs.ch);

            IReadOnlyList<UnitKeyword> keywords = _gameData.Get<UnitKeywordSettings>(_gs.ch).GetData();

            List<UnitKeyword> allKeywords = new List<UnitKeyword>();

            List<string> nameWords = unitType.Name.Split(" ").ToList();

            TribeType tribeType = _gameData.Get<TribeSettings>(_gs.ch).Get(unitType.TribeTypeId);

            if (tribeType != null)
            {
                nameWords.Add(tribeType.Name);
            }

            List<UnitEffect> spells = new List<UnitEffect>();
            List<UnitEffect> applyEffects = new List<UnitEffect>();

            List<UnitEffect> resistEffects = new List<UnitEffect>();
            List<UnitEffect> vulnEffects = new List<UnitEffect>();

            foreach (string word in nameWords)
            {
                UnitKeyword keyword = keywords.FirstOrDefault(x => x.Name == word);
                if (keyword != null)
                {
                    retval.Range = Math.Max(keyword.MinRange, retval.Range);
                    spells.AddRange(keyword.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell));
                    applyEffects.AddRange(keyword.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect));
                    resistEffects.AddRange(keyword.Effects.Where(x => x.EntityTypeId == EntityTypes.Resist));
                    vulnEffects.AddRange(keyword.Effects.Where(x => x.EntityTypeId == EntityTypes.Vulnerability));
                }
            }

            spells.AddRange(unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell));
            applyEffects.AddRange(unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect));
            resistEffects.AddRange(unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.Resist));
            vulnEffects.AddRange(unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.Vulnerability));

            // Remove duplicates
            spells = spells.GroupBy(x => x.EntityId).Select(g => g.First()).ToList();
            applyEffects = applyEffects.GroupBy(x => x.EntityId).Select(g => g.First()).ToList();


            List<FullEffect> finalApplyEffects = new List<FullEffect>();


            StatusEffectSettings statusSettings = _gameData.Get<StatusEffectSettings>(_gs.ch);

            // Don't introduce stronger debuffs until later when the player have a chance to cure them.
            double maxEffectTier = combatSettings.DebuffTiersPerUnitLevel * combatLevel;
            foreach (UnitEffect aeffect in applyEffects)
            {
                if (aeffect.EntityId > maxEffectTier)
                {
                    continue;
                }

                double levelDiff = maxEffectTier - aeffect.EntityId;

                // Chance is based on how much higher this level is than the debuff id/difficulty.
                double effectChance = Math.Min(combatSettings.MaxDebuffChance, combatSettings.MinDebuffChance + levelDiff * combatSettings.DebuffChancePerLevel);

                StatusEffect statusEffect = statusSettings.Get(aeffect.EntityId);

                if (statusEffect == null)
                {
                    continue;
                }

                CrawlerSpellEffect spellEffect = new CrawlerSpellEffect()
                {
                    EntityTypeId = aeffect.EntityTypeId,
                    EntityId = aeffect.EntityId,
                    MaxQuantity = 1,
                    MinQuantity = 1,
                    ElementTypeId = statusEffect.ElementTypeId,
                };


                FullEffect fullApplyEffect = new FullEffect()
                {
                    Effect = spellEffect,
                    InitialEffect = true,
                    PercentChance = effectChance,
                    ElementType = elementSettings.Get(statusEffect.ElementTypeId),
                };

                finalApplyEffects.Add(fullApplyEffect);
            }

            long vulnBits = 0;
            long resistBits = 0;

            foreach (UnitEffect eff in resistEffects)
            {
                resistBits |= (long)(1 << (int)eff.EntityId);
            }

            foreach (UnitEffect eff in vulnEffects)
            {
                vulnBits |= (long)(1 << (int)eff.EntityId);
            }


            bool isGuardian = spells.Any(x => x.EntityTypeId == EntityTypes.CrawlerSpell && defendSpellIds.Contains(x.EntityId));

            retval.Spells = spells;
            retval.ApplyEffects = finalApplyEffects;
            retval.ResistBits = resistBits;
            retval.VulnBits = vulnBits;
            retval.IsGuardian = isGuardian;

            return retval;
        }


        public void AddCombatUnits(PartyData partyData, UnitType unitType, long unitQuantity, long factionTypeId,
            int currRange = CrawlerCombatConstants.MinRange)
        {

            if (partyData.Combat == null)
            {
                return;
            }

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);


            List<CombatGroup> groups = factionTypeId == FactionTypes.Player ? partyData.Combat.Allies : partyData.Combat.Enemies;

            CombatGroup group = groups.FirstOrDefault(x => x.UnitTypeId == unitType.IdKey);

            long combatLevel = partyData.Combat.Level;


            FullMonsterStats fullStats = GetFullMonsterStats(unitType, combatLevel);
         
            if (group == null)
            {
                group = new CombatGroup()
                {
                    Id = partyData.GetNextGroupId(),
                    Range = currRange,
                    UnitTypeId = unitType.IdKey,
                    SingularName = unitType.Name,
                    PluralName = unitType.PluralName,
                };

                bool didAddGroup = false;
                for (int g = 0; g < groups.Count; g++)
                {
                    if (groups[g].Range >= currRange)
                    {
                        groups.Insert(g, group);
                        didAddGroup = true;
                        break;
                    }
                }

                if (!didAddGroup)
                {
                    groups.Add(group);
                }
            }

            for (int i = 0; i < unitQuantity; i++)
            {
                if (group.Units.Count >= combatSettings.MaxGroupSize)
                {
                    break;
                }

                Monster monster = new Monster(_repoService)
                {
                    Id = partyData.GetNextGroupId(),
                    UnitTypeId = unitType.IdKey,
                    Level = combatLevel,
                    Name = unitType.Name + (i + 1),
                    PortraitName = unitType.Icon,
                    FactionTypeId = factionTypeId,
                    Spells = fullStats.Spells,
                    ApplyEffects = fullStats.ApplyEffects,
                    IsGuardian = fullStats.IsGuardian,
                    ResistBits = fullStats.ResistBits,
                    VulnBits = fullStats.VulnBits,
                    CombatGroupId = group.Id,
                };
                _statService.CalcUnitStats(partyData, monster, true);

                group.Units.Add(monster);

            }

            partyData.WorldPanel?.UpdateCombatGroups();
        }

        public bool ReadyForCombat(PartyData party)
        {
            if (party.Combat == null)
            {
                return false;
            }

            foreach (CombatGroup group in party.Combat.Allies)
            {
                if (group.CombatGroupAction != ECombatGroupActions.Fight)
                {
                    continue;
                }
                foreach (CrawlerUnit unit in group.Units)
                {
                    if (unit is PartyMember member)
                    {
                        if (unit.Action == null)
                        {
                            if (!IsDisabled(unit))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public async Task EndCombatRound(PartyData party)
        {

            try
            {
                if (party.Combat == null || !ReadyForCombat(party))
                {
                    return;
                }

                party.Combat.PlayerActionsRemaining--;

                CrawlerCombatState combat = party.Combat;

                foreach (CombatGroup group in combat.Enemies)
                {
                    group.CombatGroupAction = ECombatGroupActions.None;
                    List<CrawlerUnit> dupeList = new List<CrawlerUnit>(group.Units);
                    foreach (CrawlerUnit unit in dupeList)
                    {
                        unit.Action = null;
                        if (unit.StatusEffects.HasBit(StatusEffects.Dead))
                        {
                            group.Units.Remove(unit);
                            combat.EnemiesKilled.Add(unit);
                        }

                        List<IDisplayEffect> removeEffectList = new List<IDisplayEffect>();
                        foreach (IDisplayEffect effect in unit.Effects)
                        {
                            if (effect.MaxDuration > 0)
                            {
                                effect.DurationLeft--;
                                if (effect.DurationLeft < 0)
                                {
                                    removeEffectList.Add(effect);
                                }
                            }
                        }

                        foreach (IDisplayEffect effect in removeEffectList)
                        {
                            unit.RemoveEffect(effect);
                        }
                    }
                }
                foreach (CombatGroup group in combat.Allies)
                {
                    group.CombatGroupAction = ECombatGroupActions.None; 
                    List<CrawlerUnit> dupeList = new List<CrawlerUnit>(group.Units);
                    foreach (CrawlerUnit unit in dupeList)
                    {
                        unit.Action = null;
                        if (unit.StatusEffects.HasBit(StatusEffects.Dead))
                        {
                            if (!(unit is PartyMember member))
                            {
                                group.Units.Remove(unit);
                            }
                        }
                    }
                }

                combat.Enemies = combat.Enemies.Where(x => x.Units.Count > 0).ToList();
                await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.CombatRound);
                combat.RoundsComplete++;

            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Combat ");
            }
        }

        public void SetInitialActions(PartyData party)
        {
            // Pass 1 defend and hide


            List<long> defenderRoleIds = _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(x => x.Guardian).Select(x => x.IdKey).ToList();

            foreach (CrawlerUnit unit in party.Combat.PartyGroup.Units)
            {
                if (unit.Action == null || unit.Action.IsComplete)
                {
                    continue;
                }

                unit.DefendRank = EDefendRanks.None;

                foreach (UnitRole unitRole in unit.Roles)
                {
                    if (defenderRoleIds.Contains(unitRole.RoleId))
                    {
                        unit.DefendRank = EDefendRanks.Guardian;
                        unit.IsGuardian = true;
                        break;
                    }
                }

                if (unit.Action.CombatActionId == CombatActions.Defend)
                {
                    if (unit.DefendRank == EDefendRanks.Guardian)
                    {
                        unit.DefendRank = EDefendRanks.Taunt;
                    }
                    else
                    {
                        unit.DefendRank = EDefendRanks.Defend;
                    }
                }
                else if (unit.Action.CombatActionId == CombatActions.Hide)
                {
                    unit.HideExtraRange += CrawlerCombatConstants.RangeDelta;
                }
            }

            foreach (CombatGroup cgroup in party.Combat.Allies)
            {
                if (cgroup == party.Combat.PartyGroup)
                {
                    continue;
                }

                foreach (CrawlerUnit unit in cgroup.Units)
                {
                    if (unit.IsGuardian)
                    {
                        unit.DefendRank = EDefendRanks.Guardian;
                    }
                }
            }
        }

        public bool SetMonsterActions(PartyData party)
        {
            if (party.Combat == null || !ReadyForCombat(party) || party.Combat.PartyGroup.CombatGroupAction== ECombatGroupActions.Prepare ||
                party.Combat.PlayerActionsRemaining > 1)
            {
                return false;
            }

            CrawlerCombatState combat = party.Combat;

            List<CrawlerUnit> tauntUnits = new List<CrawlerUnit>();

            foreach (CombatGroup combatGroup in combat.Allies)
            {
                tauntUnits.AddRange(combatGroup.Units.Where(x => x.DefendRank >= EDefendRanks.Guardian));
            }



            if (tauntUnits.Count > 0)
            {
                EDefendRanks maxDefendRank = tauntUnits.Max(x => x.DefendRank);
                tauntUnits = tauntUnits.Where(x => x.DefendRank == maxDefendRank).ToList();
            }


            List<CrawlerUnit> monsterTaunts = new List<CrawlerUnit>();

            foreach (CrawlerUnit unit in tauntUnits)
            {
                if (unit is Monster monster)
                {
                    monsterTaunts.Add(unit);
                }
            }
            
            if (monsterTaunts.Count > 0)
            {
                tauntUnits = monsterTaunts;
            }

            foreach (CombatGroup group in combat.Allies)
            {
                if (group != party.Combat.PartyGroup && party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Fight)
                {
                    SelectGroupActions(party, group, new List<CrawlerUnit>(), combat.Allies, combat.Enemies);
                }
            }

            foreach (CombatGroup group in combat.Enemies)
            {
                SelectGroupActions(party, group, tauntUnits, combat.Enemies, combat.Allies);
            }

            return true;
        }

        public void RemoveEndOfCombatEffects(PartyData party)
        {
            foreach (PartyMember member in party.Members)
            {
                List<IDisplayEffect> expiredEffects = member.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect &&
                    x.MaxDuration > 0).ToList();

                foreach (IDisplayEffect effect in expiredEffects)
                {
                    member.RemoveEffect(effect);
                }
            }
        }

        public void SelectGroupActions(PartyData party, CombatGroup group, List<CrawlerUnit> tauntUnits, List<CombatGroup> friends,
            List<CombatGroup> foes)
        {
            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);


            if (group.Range > CrawlerCombatConstants.MinRange && _rand.NextDouble() <  combatSettings.GroupAdvanceChance)
            {
                group.CombatGroupAction = ECombatGroupActions.Advance;
            }
            else
            {
                group.CombatGroupAction = ECombatGroupActions.Fight;

                List<CrawlerSpell> summonSpells = new List<CrawlerSpell>();
                List<CrawlerSpell> nonSummonSpells = new List<CrawlerSpell>();


                UnitType groupUnit = _gameData.Get<UnitSettings>(_gs.ch).Get(group.UnitTypeId);

                if (groupUnit != null)
                {
                    List<long> spellIds = groupUnit.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell).Select(x=>x.EntityId).ToList();
                    IReadOnlyList<CrawlerSpell> currentSpells = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().Where(x=> spellIds.Contains(x.IdKey)).ToList();

                    summonSpells = currentSpells.Where(x => x.Effects.Any(e => e.EntityTypeId == EntityTypes.Unit && e.EntityId > 0)).ToList();
                    nonSummonSpells = currentSpells.Except(summonSpells).ToList();
                }

                foreach (CrawlerUnit unit in group.Units)
                {
                    SelectMonsterAction(party, group, unit, tauntUnits, friends, foes, summonSpells, nonSummonSpells);
                }
            }
        }

        public void SelectMonsterAction(PartyData party, CombatGroup unitGroup,
            CrawlerUnit unit, List<CrawlerUnit> tauntUnits,
            List<CombatGroup> allyGroups, List<CombatGroup> enemyGroups, List<CrawlerSpell> summonSpells,
            List<CrawlerSpell> nonSummonSpells)
        {
            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            CrawlerMonsterSettings monsterSettings = _gameData.Get<CrawlerMonsterSettings>(_gs.ch);
            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(_gs.ch);


            if (party.Combat == null)
            {
                return;
            }

            if (unit.IsPlayer())
            {
                if (!unit.StatusEffects.HasBit(StatusEffects.Possessed))
                {
                    return;
                }
                else
                {
                    List<CombatGroup> temp = allyGroups;
                    allyGroups = enemyGroups;
                    enemyGroups = temp;
                    tauntUnits = new List<CrawlerUnit>();
                    return;
                }
            }

            CrawlerUnit target = null;

            if (unit.FactionTypeId != FactionTypes.Player && tauntUnits.Count > 0)
            {
                target = tauntUnits[_rand.Next() % tauntUnits.Count];
            }
            else
            {
                target = SelectRandomUnit(enemyGroups);
            }

            List<CrawlerUnit> targets = new List<CrawlerUnit>();

            if (target != null)
            {
                targets.Add(target);
            }

            UnitAction combatAction = new UnitAction()
            {
                Caster = unit,
                FinalTargets = targets,
            };

            // Only enemy monsters summon in combat
            if (!allyGroups.Contains(party.Combat.PartyGroup) && summonSpells.Count > 0 && _rand.NextDouble() < combatSettings.SummonChance)
            {
                CrawlerSpell spell = summonSpells[_rand.Next(summonSpells.Count)];

                long cost = _spellService.GetPowerCost(party, unit, spell);

                long mana = unit.Stats.Get(StatTypes.Mana, StatCategories.Curr);

                if (mana >= cost)
                {
                    combatAction.CombatActionId = CombatActions.Cast;
                    combatAction.Spell = spell;
                    combatAction.FinalTargets = new List<CrawlerUnit>() { unit };
                }
            }

            if (combatAction.Spell == null && nonSummonSpells.Count > 0 && _rand.NextDouble() < combatSettings.CastSpellChance)
            {
                CrawlerSpell spell = nonSummonSpells[_rand.Next(nonSummonSpells.Count)];

                long cost = _spellService.GetPowerCost(party, unit, spell);

                long mana = unit.Stats.Get(StatTypes.Mana, StatCategories.Curr);

                if (mana >= cost)
                {
                    combatAction.CombatActionId = CombatActions.Cast;
                    combatAction.Spell = spell;
                    combatAction.FinalTargets = targets;

                    if (!_spellService.IsEnemyTarget(spell.TargetTypeId))
                    {
                        if (spell.TargetTypeId == TargetTypes.AllAllies)
                        {
                            combatAction.FinalTargets = new List<CrawlerUnit>();
                            foreach (CombatGroup cgroup in allyGroups)
                            {
                                combatAction.FinalTargets.AddRange(cgroup.Units);
                            }
                        }
                        else
                        {
                            combatAction.FinalTargets = new List<CrawlerUnit>() { unit };
                        }
                    }
                    else
                    {
                        if (spell.TargetTypeId == TargetTypes.EnemyGroup)
                        {
                            if (enemyGroups.Count > 0)
                            {
                                CombatGroup egroup = enemyGroups[_rand.Next(enemyGroups.Count)];

                                combatAction.FinalTargets = new List<CrawlerUnit>(egroup.Units);
                            }
                        }
                        else if (spell.TargetTypeId == TargetTypes.AllEnemyGroups)
                        {
                            combatAction.FinalTargets = new List<CrawlerUnit>();
                            foreach (CombatGroup egroup in enemyGroups)
                            {
                                combatAction.FinalTargets.AddRange(egroup.Units);
                            }
                        }
                    }
                }
            }

            // Now attack if we didn't cast a spell.
            if (combatAction.Spell == null)
            {
                combatAction.Spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).Get(CrawlerSpells.AttackId);
                combatAction.CombatActionId = CombatActions.Attack;
                if (unitGroup.Range > CrawlerCombatConstants.MinRange || !enemyGroups.Any(x=>x.Range <= CrawlerCombatConstants.MinRange))
                {
                    combatAction.Spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).Get(CrawlerSpells.ShootId);
                    combatAction.CombatActionId = CombatActions.Shoot;
                }
            }

            if (combatAction.Spell != null)
            {
                unit.Action = combatAction;
            }
        }

        private CrawlerUnit SelectRandomUnit(List<CombatGroup> groups)
        {
            List<CrawlerUnit> allUnits = new List<CrawlerUnit>();

            foreach (CombatGroup group in groups)
            {
                allUnits.AddRange(group.Units);
            }

            if (allUnits.Count > 0)
            {
                return allUnits[_rand.Next() % allUnits.Count];
            }

            return null;
        }

        public UnitAction GetActionFromSpell(PartyData party, CrawlerUnit unit, CrawlerSpell spell,
            List<UnitAction> currentActions = null)
        {

            UnitAction newAction = new UnitAction()
            {
                CombatActionId = spell.CombatActionId,
                Spell = spell,
                Caster = unit,
            };

            if (spell.TargetTypeId == TargetTypes.AllAllies)
            {
                if (party.Combat != null)
                {
                    newAction.FinalTargets = new List<CrawlerUnit>();
                    foreach (CombatGroup cgroup in party.Combat.Allies)
                    {
                        newAction.FinalTargets.AddRange(cgroup.Units);
                    }
                }
                else
                {
                    newAction.FinalTargets = new List<CrawlerUnit>(party.GetActiveParty());
                }
            }
            else if (spell.TargetTypeId == TargetTypes.Ally)
            {
                newAction.PossibleTargetUnits = new List<CrawlerUnit>(party.GetActiveParty());
            }
            else if (spell.TargetTypeId == TargetTypes.Self)
            {
                newAction.FinalTargets.Add(unit);
            }
            else if (spell.TargetTypeId == TargetTypes.Special)
            {
                // No targets added here.
            }
            else // Target must be some kind of enemies.
            {
                List<CombatGroup> possibleGroups = new List<CombatGroup>();

                long minRange = spell.MinRange;
                long maxRange = spell.MaxRange;
                if (unit.HideExtraRange > 0)
                {
                    maxRange =unit.HideExtraRange + CrawlerCombatConstants.MinRange;
                }

                foreach (CombatGroup group in party.Combat.Enemies)
                {

                    if (group.Range >= minRange && group.Range <= maxRange)
                    {
                        possibleGroups.Add(group);
                    }
                }

                if (possibleGroups.Count < 1)
                {
                    return null;
                }
                else if (possibleGroups.Count > 1)
                {
                    if (spell.TargetTypeId == TargetTypes.AllEnemies || spell.TargetTypeId == TargetTypes.AllEnemyGroups)
                    {
                        for (int g = 0; g < possibleGroups.Count; g++)
                        {
                            CombatGroup group = possibleGroups[g];

                            foreach (CrawlerUnit crawlerUnit in group.Units)
                            {
                                newAction.FinalTargets.Add(crawlerUnit);
                            }
                        }
                    }
                    else
                    {
                        newAction.PossibleTargetGroups = new List<CombatGroup>(possibleGroups);
                    }

                }
                else if (possibleGroups.Count == 1)
                {
                    newAction.FinalTargets.AddRange(possibleGroups[0].Units.Select(x => x).ToList());
                }
            }

            if (spell.TargetTypeId != TargetTypes.Special &&
                newAction.FinalTargets.Count < 1 && newAction.PossibleTargetUnits.Count < 1 && newAction.PossibleTargetGroups.Count < 1)
            {
                return null;
            }
            UnitAction currAction = null;

            if (currentActions != null)
            {
                currAction = currentActions.FirstOrDefault(x => x.CombatActionId == newAction.CombatActionId);
            }

            if (currAction == null)
            {
                CombatAction combatAction = _gameData.Get<CombatActionSettings>(_gs.ch).Get(newAction.CombatActionId);
                newAction.Text = combatAction.Name;
                if (combatAction.Name != spell.Name)
                {
                    newAction.Text += ": " + spell.Name;
                }
                if (newAction.CombatActionId == CombatActions.Defend)
                {
                    if (unit.DefendRank >= EDefendRanks.Guardian)
                    {
                        newAction.Text += ": (Taunt)";
                    }

                }
            }
            else
            {
                newAction.Text = spell.Name;

                if (spell.CombatActionId == CombatActions.Hide)
                {
                    newAction.Text += "(" + (unit.DefendRank + CrawlerCombatConstants.MinRange) + "')";
                }
            }

            return newAction;
        }

        public List<UnitAction> GetActionsForPlayer(PartyData party, CrawlerUnit unit)
        {
            PartyMember member = unit as PartyMember;

            List<UnitAction> retval = new List<UnitAction>();

            if (IsDisabled(member))
            {
                retval.Add(new UnitAction()
                {
                    CombatActionId = CombatActions.Disabled,
                });
                return retval;
            }

            List<CrawlerSpell> nonCastSpells = _spellService.GetNonSpellCombatActionsForMember(party, member);

            foreach (CrawlerSpell spell in nonCastSpells)
            {
                UnitAction newAction = GetActionFromSpell(party, unit, spell, retval);
                if (newAction != null)
                {
                    retval.Add(newAction);
                }
            }

            List<CrawlerSpell> spells = _spellService.GetSpellsForMember(party, member);

            if (spells.Count > 0)
            {
                retval.Add(new UnitAction() { Caster = member, CombatActionId = CombatActions.Cast, Text = "Cast" });
            }

            if (party.Combat != null)
            {
                CrawlerSpell prevSpell = spells.FirstOrDefault(x => x.IdKey == member.LastCombatCrawlerSpellId);
                if (prevSpell == null)
                {
                    prevSpell = nonCastSpells.FirstOrDefault(x => x.IdKey == member.LastCombatCrawlerSpellId);
                }

                if (prevSpell != null)
                {

                    retval.Add(new UnitAction() { Caster = member, Spell = prevSpell, CombatActionId = CombatActions.Recast });
                }
            }

            if (retval.Count < 1)
            {
                retval.Add(new UnitAction() { Caster = member, CombatActionId = CombatActions.Disabled });
            }

            return retval;
        }

        private DateTime _lastMoveTime = DateTime.UtcNow;
        private int _movesSinceLastCombat = 0;
        public void CheckForEncounter(bool atEndOfMove, CancellationToken token)
        {

            if (atEndOfMove)
            {
                CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);

                PartyData party = _crawlerService.GetParty();

                CrawlerMap map = _worldService.GetMap(party.MapId);

                bool newlyMarked = false;

                if (!party.CompletedMaps.HasBit(party.MapId))
                {
                    CrawlerMapStatus status = party.Maps.FirstOrDefault(x => x.MapId == party.MapId);

                    if (status == null || !status.Visited.HasBit(map.GetIndex(party.MapX, party.MapZ)))
                    {
                        newlyMarked = true;
                    }
                }

                CrawlerMap cmap = _worldService.GetMap(party.MapId);

                _lastMoveTime = DateTime.UtcNow;
                if (++_movesSinceLastCombat < combatSettings.MovesBetweenEncounters)
                {
                    return;
                }


                double randomChance = combatSettings.RandomEncounterChance;

                if (_gs.GameMode == EGameModes.Roguelike)
                {
                    randomChance *= _gameData.Get<RoguelikeSettings>(_gs.ch).RandomEncounterChanceMult;
                }
                if (_rand.NextDouble() > randomChance)
                {
                    return;
                }

                if (!newlyMarked && _gs.GameMode != EGameModes.Roguelike)
                {
                    return;
                }

            }
            else // Just idle waiting.
            {
                if ((DateTime.UtcNow - _lastMoveTime).TotalSeconds < 60)
                {
                    return;
                }

                if (_rand.NextDouble() > 0.002f)
                {
                    return;
                }
                return;
            }

            _crawlerMapService.ClearMovement();
            _crawlerService.ChangeState(ECrawlerStates.StartCombat, token);
            _movesSinceLastCombat = 0;
        }


        private int _disabledBits = -1;
        public bool IsDisabled(CrawlerUnit unit)
        {
            if (_disabledBits < 0)
            {
                _disabledBits = 0;
                IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

                foreach (StatusEffect eff in effects)
                {
                    if (eff.ActionEffects.Count > 0)
                    {
                        if (eff.ActionEffects.Any(x => x.Effect == EActionEffects.Set &&
                            x.CombatActionId == CombatActions.Disabled))
                        {
                            _disabledBits |= 1 << (int)eff.IdKey;
                        }
                    }
                }
            }

            return unit.HasStatusBits(_disabledBits);
        }

        private Dictionary<long, int> _actionToDisableBits = new Dictionary<long, int>()
        {
            [CombatActions.Attack] = MapDisables.NoMelee,
            [CombatActions.Shoot] = MapDisables.NoRanged,
            [CombatActions.Cast] = MapDisables.NoMagic,
        };

        Dictionary<long, long> _combatActionBlocks = new Dictionary<long, long>();
        public bool IsActionBlocked(PartyData party, CrawlerUnit unit, long combatActionId)
        {

            int disabledBits = _worldService.GetMap(party.MapId)?.Get(party.MapX, party.MapZ, CellIndex.Disables) ?? 0;

            if (_actionToDisableBits.ContainsKey(combatActionId) &&
                FlagUtils.IsSet(_actionToDisableBits[combatActionId], disabledBits))
            {
                return true;
            }


            if (!_combatActionBlocks.ContainsKey(combatActionId))
            {
                _combatActionBlocks[combatActionId] = 0;
                IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

                foreach (StatusEffect eff in effects)
                {
                    if (eff.ActionEffects.Count > 0)
                    {
                        if (eff.ActionEffects.Any(x => x.Effect == EActionEffects.Block &&
                            x.CombatActionId == combatActionId))
                        {
                            _combatActionBlocks[combatActionId] += 1 << (int)eff.IdKey;
                        }
                    }
                }
            }

            return unit.HasStatusBits(_combatActionBlocks[combatActionId]);

        }

        Dictionary<long, long> _combatActionWeaks = new Dictionary<long, long>();
        public bool IsActionWeak(CrawlerUnit unit, long combatActionId)
        {
            if (!_combatActionWeaks.ContainsKey(combatActionId))
            {
                _combatActionWeaks[combatActionId] = 0;
                IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

                foreach (StatusEffect eff in effects)
                {
                    if (eff.ActionEffects.Count > 0)
                    {
                        if (eff.ActionEffects.Any(x => x.Effect == EActionEffects.Block &&
                            x.CombatActionId == combatActionId))
                        {
                            _combatActionWeaks[combatActionId] += 1 << (int)eff.IdKey;
                        }
                    }
                }
            }

            return unit.HasStatusBits(_combatActionWeaks[combatActionId]);

        }
    }
}
