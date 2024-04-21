using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Combat.Utils;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Services.Combat
{
    public class CombatService : ICombatService
    {
        private ICrawlerStatService _statService;
        private ICrawlerSpellService _spellService;
        protected IGameData _gameData;

        public async Task Initialize(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public bool StartCombat(GameState gs, PartyData partyData, CombatState combatState)
        {
            if (partyData.Combat != null)
            {
                return false;
            }

            partyData.Combat = combatState;

            List<PartyMember> members = partyData.GetActiveParty();

            foreach (PartyMember member in members)
            {
                foreach (PartySummon summon in member.Summons)
                {
                    UnitType unitType = _gameData.Get<UnitSettings>(null).Get(summon.UnitTypeId);

                    if (unitType != null)
                    {
                        AddCombatUnits(gs, partyData, unitType, 1, FactionTypes.Player);
                    }
                }
            }

            CombatGroup partyGroup = new CombatGroup() { SingularName = "Player", PluralName = "Players" };
            combatState.Allies.Add(partyGroup);
            combatState.PartyGroup = partyGroup;

            foreach (PartyMember member in members)
            {
                partyGroup.Units.Add(member);           
            }

            IReadOnlyList<UnitType> allUnitTypes = _gameData.Get<UnitSettings>(null).GetData();


            int groupCount = CrawlerCombatConstants.MaxStartCombatGroups;

            for (int i = 0; i < 2; i++)
            {
                // Maybe make a min later.
                groupCount = MathUtils.IntRange(1, CrawlerCombatConstants.MaxStartCombatGroups, gs.rand);
            }

            if (groupCount > combatState.Level/2+1)
            {
                groupCount = (int)(combatState.Level / 2 + 1);
            }

            List<UnitType> chosenUnitTypes = new List<UnitType>();

            while (chosenUnitTypes.Count < groupCount)
            {
                UnitType utype = allUnitTypes[gs.rand.Next() % allUnitTypes.Count];
                if (utype.IdKey > 0)
                {
                    chosenUnitTypes.Add(utype);
                }
            }

            int currRange = CrawlerCombatConstants.MinRange;
            foreach (UnitType unitType in chosenUnitTypes)
            {
                int quantity = MathUtils.IntRange(1, 8, gs.rand);

                while (gs.rand.NextDouble() < 0.2f)
                {
                    quantity += MathUtils.IntRange(1, 4, gs.rand);
                }

                if (combatState.Level >= 10)
                {
                    while (gs.rand.NextDouble() < 0.1f)
                    {
                        quantity += MathUtils.IntRange(10, 20, gs.rand);
                    }
                }

                if (quantity > 99)
                {
                    quantity = 99;
                }

                AddCombatUnits(gs, partyData, unitType, quantity, FactionTypes.Faction1);

                if (gs.rand.NextDouble() < 0.9f)
                {
                    currRange += CrawlerCombatConstants.RangeDelta * 2;
                }
            }

            return true;
        }

        public void AddCombatUnits(GameState gs, PartyData partyData, UnitType unitType, long unitQuantity, long factionTypeId,
            int currRange = CrawlerCombatConstants.MinRange)
        {

            if (partyData.Combat == null)
            {
                return;
            }

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);

            List<CombatGroup> groups = (factionTypeId == FactionTypes.Player ? partyData.Combat.Allies : partyData.Combat.Enemies);

            CombatGroup group = groups.FirstOrDefault(x => x.UnitTypeId == unitType.IdKey);

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

                Monster monster = new Monster()
                {
                    UnitTypeId = unitType.IdKey,
                    Level = partyData.Combat.Level,
                    Name = unitType.Name + (i + 1),
                    PortraitName = unitType.Icon,
                    FactionTypeId = factionTypeId,
                };
                _statService.CalcUnitStats(gs, partyData, monster, true);

                group.Units.Add(monster);

            }
        }

        public bool ReadyForCombat(GameState gs, PartyData party)
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

                            if (CombatUtils.CanPerformAction(unit))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public void EndCombatRound (GameState gs, PartyData party)
        {
            if (party.Combat == null || !ReadyForCombat(gs, party))
            {
                return;
            }

            CombatState combat = party.Combat;

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
                foreach (CrawlerUnit unit in group.Units)
                {
                    unit.Action = null;
                }
            }

            combat.Enemies = combat.Enemies.Where(x => x.Units.Count > 0).ToList();

        }

        public void SetInitialActions(GameState gs, PartyData party)
        {
            // Pass 1 defend and hide

            foreach (CrawlerUnit unit in party.Combat.PartyGroup.Units)
            {
                if (unit.Action == null || unit.Action.IsComplete)
                {
                    continue;
                }
                unit.DefendRank = EDefendRanks.None;                

                if (unit.Action.CombatActionId == CombatActions.Defend)
                {
                    if (unit.Action.Spell.ReplacesCrawlerSpellId == 0)
                    {
                        unit.DefendRank = EDefendRanks.Defend;
                    }
                    else
                    {
                        unit.DefendRank = EDefendRanks.Taunt;
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

        public bool SetMonsterActions(GameState gs, PartyData party)
        {
            if (party.Combat == null || !ReadyForCombat(gs, party))
            {
                return false;
            }
            List<CrawlerSpell> monsterSpells = _gameData.Get<CrawlerSpellSettings>(null).GetData().Where(x => x.HasFlag(CrawlerSpellFlags.MonsterOnly)).ToList();


            CombatState combat = party.Combat;

            List<CrawlerUnit> tauntUnits = new List<CrawlerUnit>();

            foreach (CombatGroup combatGroup in combat.Allies)
            {
                foreach (CrawlerUnit unit in  combatGroup.Units)
                {
                    if (unit.DefendRank == EDefendRanks.Taunt)
                    {
                        tauntUnits.Add(unit);
                    }
                }
            }

            foreach (CombatGroup group in combat.Allies)
            {
                SelectGroupActions(gs, party, group, new List<CrawlerUnit>(), combat.Allies, combat.Enemies, monsterSpells);
            }

            foreach (CombatGroup group in combat.Enemies)
            {
                SelectGroupActions(gs, party, group, tauntUnits, combat.Enemies, combat.Allies, monsterSpells);
            }

            return true;
        }

        public void RemoveEndOfCombatEffects(GameState gs, PartyData party)
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

        public void SelectGroupActions(GameState gs, PartyData party, CombatGroup group, List<CrawlerUnit> tauntUnits, List<CombatGroup> friends,
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
                    SelectMonsterAction(gs, party, unit, tauntUnits, friends, foes, monsterSpells);
                }
            }
        }

        public void SelectMonsterAction (GameState gs, PartyData party,
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
                target = tauntUnits[gs.rand.Next() % tauntUnits.Count];
            }
            else
            {
                target = SelectRandomUnit(gs, foes);
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
                CombatActionId = (targets.Count > 0 ? CombatActions.Attack : CombatActions.Defend),          
            };



            if (combatAction.CombatActionId == CombatActions.Attack)
            {
                combatAction.Spell = _gameData.Get<CrawlerSpellSettings>(null).Get(CrawlerSpells.AttackId);
            }
            else
            {
                combatAction.Spell = _gameData.Get<CrawlerSpellSettings>(null).Get(CrawlerSpells.DefendId);
            }

            if (!unit.IsPlayer() && monsterSpells.Count > 0 && gs.rand.NextDouble() < 0.05f)
            {
                CrawlerSpell spell = monsterSpells[gs.rand.Next() % monsterSpells.Count];
                if (spell.TargetTypeId == TargetTypes.Self)
                {
                    combatAction.FinalTargets = new List<CrawlerUnit>() { unit };
                    combatAction.Spell = spell;
                    combatAction.CombatActionId = CombatActions.Cast;
                }
            }



            unit.Action = combatAction;
        }

        private CrawlerUnit SelectRandomUnit(GameState gs, List<CombatGroup> groups)
        {
            List<CrawlerUnit> allUnits = new List<CrawlerUnit>();

            foreach (CombatGroup group in groups)
            {
                allUnits.AddRange(group.Units);
            }

            if (allUnits.Count > 0)
            {
                return allUnits[gs.rand.Next() % allUnits.Count];
            }

            return null;
        }

        public UnitAction GetActionFromSpell(GameState gs, PartyData party, CrawlerUnit unit, CrawlerSpell spell,
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
                    if (spell.TargetTypeId == TargetTypes.AllEnemies)
                    {
                        foreach (CombatGroup group in possibleGroups)
                        {
                            newAction.FinalTargets.AddRange(group.Units.Select(x => x));
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
            }
            else
            {
                newAction.Text = spell.Name;

                if (spell.CombatActionId == CombatActions.Hide)
                {
                    newAction.Text += "("+(unit.DefendRank + CrawlerCombatConstants.MinRange) + "')";
                }
            }

            return newAction;
        }

        public List<UnitAction> GetActionsForPlayer(GameState gs, PartyData party, CrawlerUnit unit)
        {
            PartyMember member = unit as PartyMember;

            List<UnitAction> retval = new List<UnitAction>();

            if (!CombatUtils.CanPerformAction(member))
            {
                retval.Add(new UnitAction()
                {
                    CombatActionId = CombatActions.Disabled,
                });
                return retval;
            }

            List<CrawlerSpell> nonCastSpells = _spellService.GetNonSpellCombatActionsForMember(gs, party, member, true);

            foreach (CrawlerSpell spell in nonCastSpells)
            {
                UnitAction newAction = GetActionFromSpell(gs, party, unit, spell, retval);
                if (newAction != null)
                {
                    retval.Add(newAction);
                }
            }

            List<CrawlerSpell> spells = _spellService.GetSpellsForMember(gs, party, member, true);

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

      
    }
}
