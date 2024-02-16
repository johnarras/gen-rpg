using Assets.Scripts.UI.Crawler;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Combat.Utils;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Settings.Targets;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;
using UnityEngine.Assertions.Must;
using UnityEngine.XR.WSA;

namespace Assets.Scripts.Crawler.Services.Combat
{
    public class CombatService : ICombatService
    {
        private ICrawlerStatService _statService;
        private ICrawlerSpellService _spellService;

        public async Task Setup(GameState gs, CancellationToken token)
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

            CombatGroup partyGroup = new CombatGroup() { SingularName = "Player", PluralName = "Players" };
            combatState.Allies.Add(partyGroup);

            List<PartyMember> members = partyData.GetActiveParty();

            foreach (PartyMember member in members)
            {
                partyGroup.Units.Add(member);           
            }

            IReadOnlyList<UnitType> allUnitTypes = gs.data.Get<UnitSettings>(null).GetData();


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

                string singularName = unitType.Name;
                string pluralName = unitType.PluralName;

                // Should modify the monsters here.


                CombatGroup group = new CombatGroup() { Range = currRange };

                group.SingularName = singularName;
                group.PluralName = pluralName;
                
                combatState.Enemies.Add(group);

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

                for (int i = 0; i < quantity; i++)
                {

                    Monster monster = new Monster() 
                    { 
                        UnitTypeId = unitType.IdKey, 
                        Level = combatState.Level, 
                        Name = singularName + (i+1),
                        PortraitName = unitType.Icon,
                    };
                    _statService.CalcUnitStats(gs, partyData, monster, true);

                    group.Units.Add(monster);

                }
                if (gs.rand.NextDouble() < 0.9f)
                {
                    currRange += CrawlerCombatConstants.RangeDelta * 2;
                }

            }

            return true;
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

            foreach (CrawlerUnit unit in party.Combat.Allies[0].Units)
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
                SelectGroupActions(gs, party, group, tauntUnits, combat.Allies, combat.Enemies);
            }

            foreach (CombatGroup group in combat.Enemies)
            {
                SelectGroupActions(gs, party, group, new List<CrawlerUnit>(), combat.Enemies, combat.Allies);
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
            List<CombatGroup> foes)
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
                    SelectMonsterAction(gs, party, unit, tauntUnits, friends, foes);
                }
            }
        }

        public void SelectMonsterAction (GameState gs, PartyData party,
            CrawlerUnit unit, List<CrawlerUnit> tauntUnits,
            List<CombatGroup> friends, List<CombatGroup> foes)
        {
            if (party.Combat == null)
            {
                return;
            }

            if (unit is PartyMember member)
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
                }
            }

            CrawlerUnit target = null;

            if (tauntUnits.Count > 0)
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
                combatAction.Spell = gs.data.Get<CrawlerSpellSettings>(null).Get(CrawlerSpells.AttackId);
            }
            else
            {
                combatAction.Spell = gs.data.Get<CrawlerSpellSettings>(null).Get(CrawlerSpells.DefendId);
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

            if (spell.TargetTypeId == TargetTypes.Party)
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
                CombatAction combatAction = gs.data.Get<CombatActionSettings>(null).Get(newAction.CombatActionId);
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
