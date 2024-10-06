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
using Genrpg.Shared.Crawler.Stats.Utils;
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
    }
    public class CrawlerCombatService : ICrawlerCombatService
    {
        private ICrawlerStatService _statService;
        private ICrawlerSpellService _spellService;
        protected IGameData _gameData;
        private IRepositoryService _repoService;
        protected IClientGameState _gs;
        protected IClientRandom _rand;
        private ICrawlerMapService _crawlerMapService;
        private ICrawlerService _crawlerService;
        private ICrawlerWorldService _worldService;
        private ILogService _logService;
        private ITimeOfDayService _timeService;

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

            foreach (PartyMember member in members)
            {
                List<Role> roles = roleSettings.GetRoles(member.Roles);

                // 1.5 here for rounding and not random scaling value combat to combat
                long quantity = (long)(1.5 + roleSettings.GetScalingBonusPerLevel(roles.Select(x => x.SummonScaling).ToList()) * member.GetAbilityLevel());

                long luckBonus = CrawlerStatUtils.GetStatBonus(member, StatTypes.Luck);

                long luckySummonCount = 0;
                for (int q = 0; q < quantity; q++)
                {
                    if (_rand.NextDouble() * 100 < luckBonus)
                    {
                        luckySummonCount++;
                    }
                }
                quantity += luckySummonCount;

                foreach (PartySummon summon in member.Summons)
                {
                    UnitType unitType = _gameData.Get<UnitSettings>(null).Get(summon.UnitTypeId);

                    if (unitType != null)
                    {
                        AddCombatUnits(party, unitType, 1, FactionTypes.Player);
                    }
                }
            }

            CombatGroup partyGroup = new CombatGroup() { SingularName = "Player", PluralName = "Players" };
            party.Combat.Allies.Add(partyGroup);
            party.Combat.PartyGroup = partyGroup;

            IReadOnlyList<UnitType> allUnitTypes = _gameData.Get<UnitSettings>(null).GetData();

            foreach (PartyMember member in members)
            {
                partyGroup.Units.Add(member);
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

            return true;
        }

        public void AddCombatUnits(PartyData partyData, UnitType unitType, long unitQuantity, long factionTypeId,
            int currRange = CrawlerCombatConstants.MinRange)
        {
            if (partyData.Combat == null)
            {
                return;
            }
            IReadOnlyList<CrawlerSpell> crawlerSpells = _gameData.Get<CrawlerSpellSettings>(null).GetData();

            IReadOnlyList<StatusEffect> statusEffects = _gameData.Get<StatusEffectSettings>(null).GetData();

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);

            List<CombatGroup> groups = factionTypeId == FactionTypes.Player ? partyData.Combat.Allies : partyData.Combat.Enemies;

            CombatGroup group = groups.FirstOrDefault(x => x.UnitTypeId == unitType.IdKey);

            IReadOnlyList<UnitKeyword> keywords = _gameData.Get<UnitKeywordSettings>(null).GetData();

            List<UnitKeyword> allKeywords = new List<UnitKeyword>();

            List<string> nameWords = unitType.Name.Split(" ").ToList();

            TribeType tribeType = _gameData.Get<TribeSettings>(null).Get(unitType.TribeTypeId);

            if (tribeType != null)
            {
                nameWords.Add(tribeType.Name);
            }

            List<UnitEffect> spells = new List<UnitEffect>();
            List<UnitEffect> applyEffects = new List<UnitEffect>();

            foreach (string word in nameWords)
            {
                UnitKeyword keyword = keywords.FirstOrDefault(x => x.Name == word);
                if (keyword != null)
                {
                    currRange = Math.Max(keyword.MinRange, currRange);
                    spells.AddRange(keyword.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell));
                    applyEffects.AddRange(keyword.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect));
                }
            }

            // Remove duplicates
            spells = spells.GroupBy(x => x.EntityId).Select(g => g.First()).ToList();
            applyEffects = applyEffects.GroupBy(x => x.EntityId).Select(g => g.First()).ToList();

            if (true)
            {
                StringBuilder sb = new StringBuilder();

                if (spells.Count > 0)
                {
                    sb.Append("Spells: ");
                    foreach (UnitEffect effect in spells)
                    {
                        CrawlerSpell spell = crawlerSpells.FirstOrDefault(x => x.IdKey == effect.EntityId);
                        if (spell != null)
                        {
                            sb.Append(" [" + spell.Name + "] ");
                        }
                    }
                }

                if (applyEffects.Count > 0)
                {
                    sb.Append("Apply Effects: ");
                    foreach (UnitEffect effect in applyEffects)
                    {
                        StatusEffect eff = statusEffects.FirstOrDefault(x => x.IdKey == effect.EntityId);
                        if (eff != null)
                        {
                            sb.Append(" [" + eff.Name + "] ");
                        }
                    }
                }

                _logService.Info(sb.ToString());
            }

            if (group == null)
            {
                group = new CombatGroup()
                {
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
                    UnitTypeId = unitType.IdKey,
                    Level = partyData.Combat.Level,
                    Name = unitType.Name + (i + 1),
                    PortraitName = unitType.Icon,
                    FactionTypeId = factionTypeId,
                    Spells = spells,
                    ApplyEffects = applyEffects,
                };
                _statService.CalcUnitStats(partyData, monster, true);

                group.Units.Add(monster);

            }
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

            int step = 1;
            try
            {
                step = 2;
                if (party.Combat == null || !ReadyForCombat(party))
                {
                    return;
                }
                step = 3;
                CrawlerCombatState combat = party.Combat;

                foreach (CombatGroup group in combat.Enemies)
                {
                    step = 4;
                    group.CombatGroupAction = ECombatGroupActions.None;
                    List<CrawlerUnit> dupeList = new List<CrawlerUnit>(group.Units);
                    foreach (CrawlerUnit unit in dupeList)
                    {
                        unit.Action = null;
                        if (unit.StatusEffects.HasBit(StatusEffects.Dead))
                        {
                            step = 5;
                            group.Units.Remove(unit);
                            combat.EnemiesKilled.Add(unit);
                        }

                        List<IDisplayEffect> removeEffectList = new List<IDisplayEffect>();
                        foreach (IDisplayEffect effect in unit.Effects)
                        {
                            step = 6;
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
                            step = 7;
                            unit.RemoveEffect(effect);
                        }
                    }
                }
                step = 8;
                foreach (CombatGroup group in combat.Allies)
                {
                    group.CombatGroupAction = ECombatGroupActions.None;
                    foreach (CrawlerUnit unit in group.Units)
                    {
                        unit.Action = null;
                    }
                    step = 9;
                }

                combat.Enemies = combat.Enemies.Where(x => x.Units.Count > 0).ToList();
                step = 10;
                await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.CombatRound);
                step = 11;
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Combat " + step);
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
                    unit.StatusEffects.SetBit(StatusEffects.Hidden);
                }
                else if (unit.HideExtraRange > 0 && unit.Action.CombatActionId != CombatActions.Hide)
                {
                    unit.HideExtraRange = CrawlerCombatConstants.MinRange;
                    unit.StatusEffects.RemoveBit(StatusEffects.Hidden);
                }
            }
        }

        public bool SetMonsterActions(PartyData party)
        {
            if (party.Combat == null || !ReadyForCombat(party))
            {
                return false;
            }
            List<CrawlerSpell> monsterSpells = _gameData.Get<CrawlerSpellSettings>(null).GetData().Where(x => x.HasFlag(CrawlerSpellFlags.MonsterOnly)).ToList();

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

            foreach (CombatGroup group in combat.Allies)
            {
                if (group != party.Combat.PartyGroup && party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Fight)
                {
                    SelectGroupActions(party, group, new List<CrawlerUnit>(), combat.Allies, combat.Enemies, monsterSpells);
                }
            }

            foreach (CombatGroup group in combat.Enemies)
            {
                SelectGroupActions(party, group, tauntUnits, combat.Enemies, combat.Allies, monsterSpells);
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
            List<CombatGroup> foes, List<CrawlerSpell> monsterSpells)
        {

            if (group.Range > CrawlerCombatConstants.MinRange)
            {
                group.CombatGroupAction = ECombatGroupActions.Advance;
            }
            else
            {
                group.CombatGroupAction = ECombatGroupActions.Fight;

                foreach (CrawlerUnit unit in group.Units)
                {
                    SelectMonsterAction(party, unit, tauntUnits, friends, foes, monsterSpells);
                }
            }
        }

        public void SelectMonsterAction(PartyData party,
            CrawlerUnit unit, List<CrawlerUnit> tauntUnits,
            List<CombatGroup> friends, List<CombatGroup> foes, List<CrawlerSpell> monsterSpells)
        {
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
                    List<CombatGroup> temp = friends;
                    friends = foes;
                    foes = temp;
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
                target = SelectRandomUnit(foes);
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
                CombatActionId = targets.Count > 0 ? CombatActions.Attack : CombatActions.Defend,
            };



            if (combatAction.CombatActionId == CombatActions.Attack)
            {
                combatAction.Spell = _gameData.Get<CrawlerSpellSettings>(null).Get(CrawlerSpells.AttackId);
            }
            else
            {
                combatAction.Spell = _gameData.Get<CrawlerSpellSettings>(null).Get(CrawlerSpells.DefendId);
            }

            if (!unit.IsPlayer() && monsterSpells.Count > 0 && _rand.NextDouble() < 0.05f)
            {
                CrawlerSpell spell = monsterSpells[_rand.Next() % monsterSpells.Count];
                if (spell.TargetTypeId == TargetTypes.Self)
                {
                    combatAction.FinalTargets = new List<CrawlerUnit>() { unit };
                    combatAction.Spell = spell;
                    combatAction.CombatActionId = CombatActions.Cast;
                }
            }



            unit.Action = combatAction;
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
                newAction.FinalTargets = new List<CrawlerUnit>(party.GetActiveParty());
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
                if (spell.RequiredStatusEffectId == StatusEffects.Hidden)
                {
                    maxRange = Math.Max(maxRange, unit.HideExtraRange + CrawlerCombatConstants.MinRange);
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
                                crawlerUnit.CombatGroupId = g;
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
                CombatAction combatAction = _gameData.Get<CombatActionSettings>(null).Get(newAction.CombatActionId);
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

            List<CrawlerSpell> nonCastSpells = _spellService.GetNonSpellCombatActionsForMember(party, member, true);

            foreach (CrawlerSpell spell in nonCastSpells)
            {
                UnitAction newAction = GetActionFromSpell(party, unit, spell, retval);
                if (newAction != null)
                {
                    retval.Add(newAction);
                }
            }

            List<CrawlerSpell> spells = _spellService.GetSpellsForMember(party, member, true);

            if (spells.Count > 0)
            {
                retval.Add(new UnitAction() { Caster = member, CombatActionId = CombatActions.Cast, Text = "Cast" });
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
                if (++_movesSinceLastCombat < 10)
                {
                    return;
                }

                if (_rand.NextDouble() > 0.03f)
                {
                    return;
                }

                if (!newlyMarked)
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
                IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(null).GetData();

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
                IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(null).GetData();

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
                IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(null).GetData();

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
