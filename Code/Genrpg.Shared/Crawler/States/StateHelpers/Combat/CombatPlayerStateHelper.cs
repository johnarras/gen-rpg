using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roguelikes.Constants;
using Genrpg.Shared.Crawler.Roguelikes.Settings;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Combat
{
    public class CombatPlayerStateHelper : BaseCombatStateHelper
    {

        public override ECrawlerStates GetKey() { return ECrawlerStates.CombatPlayer; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {

            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            if (party.Combat == null || party.Combat.Allies.Count < 1 ||
                (party.Combat.PartyGroup.CombatGroupAction != ECombatGroupActions.Fight &&
                party.Combat.PartyGroup.CombatGroupAction != ECombatGroupActions.Prepare))
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Party is not fighting in combat" };
            }

            if (party.Combat.PlayerActionsRemaining < 1)
            {
                party.Combat.PlayerActionsRemaining = 1;
            }

            if (party.GameMode == EGameModes.Roguelike)
            {
                
                double extraActions = (long)_roguelikeUpgradeService.GetBonus(party, RoguelikeUpgrades.ActionCount);
                party.Combat.PlayerActionsRemaining += (long)(extraActions);

                double remainder = extraActions - (long)(extraActions);

                if (_rand.NextDouble() < remainder)
                {
                    party.Combat.PlayerActionsRemaining++;
                }
            }

            CombatGroup group = party.Combat.PartyGroup;

            List<CrawlerUnit> readyUnits = group.Units.Where(x => x is PartyMember member && x.Action != null).ToList();

            List<CrawlerUnit> notReadyUnits = group.Units.Where(x => x is PartyMember member && x.Action == null).ToList();


            if (notReadyUnits.Count < 1)
            {
                return new CrawlerStateData(ECrawlerStates.CombatConfirm, true);
            }


            stateData.WorldSpriteName = notReadyUnits[0].PortraitName;
            List<UnitAction> actions = _combatService.GetActionsForPlayer(party, notReadyUnits[0]);

            if (actions.Count == 1 && actions[0].CombatActionId == CombatActions.Disabled)
            {
                notReadyUnits[0].Action = actions[0];
                return new CrawlerStateData(ECrawlerStates.CombatPlayer, true);
            }

            PartyMember notReadyMember = notReadyUnits[0] as PartyMember;

            stateData.Actions.Add(new CrawlerStateAction("Select an action for " + _textService.HighlightText(notReadyMember.Name)));

            List<char> usedKeyCodes = new List<char>();

            foreach (UnitAction unitCombatAction in actions)
            {
                CombatAction combatAction = _gameData.Get<CombatActionSettings>(null).Get(unitCombatAction.CombatActionId);
                ECrawlerStates nextState = ECrawlerStates.CombatPlayer;
                if (unitCombatAction.CombatActionId == CombatActions.Cast && notReadyMember != null)
                {
                    nextState = ECrawlerStates.SelectSpell;

                    SelectAction select = new SelectAction()
                    {
                        Member = notReadyMember,
                        Action = unitCombatAction,
                        ReturnState = ECrawlerStates.CombatPlayer,
                        NextState = ECrawlerStates.CombatPlayer,
                    };

                    stateData.Actions.Add(new CrawlerStateAction(unitCombatAction.Text, GetKeyCode(usedKeyCodes, combatAction, unitCombatAction),
                        nextState, extraData: select));
                    continue;
                }
                else if (unitCombatAction.CombatActionId == CombatActions.Recast)
                {
                    SelectAction selectAction = new SelectAction()
                    {
                        Action = unitCombatAction,
                        Member = notReadyMember,
                        NextState = ECrawlerStates.CombatPlayer,
                        ReturnState = ECrawlerStates.CombatPlayer,
                    };

                    SelectSpellAction selectSpell = new SelectSpellAction()
                    {
                        Action = selectAction,
                        Spell = unitCombatAction.Spell,
                        PowerCost = unitCombatAction.Spell.GetPowerCost(notReadyMember.Level),
                    };

                    stateData.Actions.Add(new CrawlerStateAction("Recast " + unitCombatAction.Spell.Name, 'R', ECrawlerStates.OnSelectSpell,
                        extraData: selectSpell, forceButton:true));
                    continue;
                }
                else if (unitCombatAction.Spell == null)
                {
                    continue;
                }
                if (unitCombatAction.CombatActionId == CombatActions.Defend)
                {
                    nextState = ECrawlerStates.CombatPlayer;
                    stateData.Actions.Add(new CrawlerStateAction(unitCombatAction.Text, GetKeyCode(usedKeyCodes, combatAction, unitCombatAction),
                        nextState,
                        onClickAction: delegate ()
                        {
                            notReadyUnits[0].Action = unitCombatAction;
                        }));
                    continue;
                }
                else if (unitCombatAction.FinalTargets.Count < 1)
                {
                    if (unitCombatAction.PossibleTargetGroups.Count > 0)
                    {
                        SelectAction selectAction = new SelectAction()
                        {
                            Action = unitCombatAction,
                            Member = notReadyMember,
                            ReturnState = ECrawlerStates.CombatPlayer,
                            NextState = ECrawlerStates.CombatPlayer,
                        };

                        SelectSpellAction selectSpell = new SelectSpellAction()
                        {
                            Spell = unitCombatAction.Spell,
                            Action = selectAction,
                        };


                        stateData.Actions.Add(new CrawlerStateAction(unitCombatAction.Text, GetKeyCode(usedKeyCodes, combatAction, unitCombatAction),
                            ECrawlerStates.SelectEnemyGroup, extraData: selectSpell));
                        continue;
                    }
                    else if (unitCombatAction.Spell.TargetTypeId == TargetTypes.Ally)
                    {
                        SelectAction selectAction = new SelectAction()
                        {
                            Action = unitCombatAction,
                            Member = notReadyMember,
                            ReturnState = ECrawlerStates.CombatPlayer,
                            NextState = ECrawlerStates.CombatPlayer,
                        };

                        SelectSpellAction selectSpell = new SelectSpellAction()
                        {
                            Spell = unitCombatAction.Spell,
                            Action = selectAction,
                        };


                        stateData.Actions.Add(new CrawlerStateAction(unitCombatAction.Text, GetKeyCode(usedKeyCodes, combatAction, unitCombatAction),
                            nextState, extraData: selectSpell));
                        continue;
                    }
                    else
                    {
                        return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Bad target list for combat spell" };
                    }
                }
                else
                {

                }

                Action nextClickAction = null;

                if (nextState == ECrawlerStates.CombatPlayer)
                {
                    nextClickAction = delegate { notReadyUnits[0].Action = unitCombatAction; };
                }
                stateData.Actions.Add(new CrawlerStateAction(unitCombatAction.Text, GetKeyCode(usedKeyCodes, combatAction, unitCombatAction),
                    nextState, nextClickAction, unitCombatAction));
            }


            if (readyUnits.Count == 0)
            {
                stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.CombatFightRun,
                    onClickAction: delegate ()
                    {
                        group.CombatGroupAction = ECombatGroupActions.None;
                    }));
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.CombatPlayer,
                    onClickAction: delegate ()
                    {
                        readyUnits.Last().Action = null;
                    }));
            }

            await Task.CompletedTask;
            return stateData;
        }

        private char GetKeyCode(List<char> usedKeyCodes, CombatAction combatAction, UnitAction unitAction)
        {

            char newKeyCode = char.ToLower(combatAction.Name[0]);
            if (usedKeyCodes.Contains(newKeyCode))
            {
                newKeyCode = char.ToLower(unitAction.Spell.Name[0]);

                if (!usedKeyCodes.Contains(newKeyCode))
                {
                    usedKeyCodes.Add(newKeyCode);
                }
            }
            else
            {
                usedKeyCodes.Add(newKeyCode);
            }
            return (char)newKeyCode;
        }
    }
}
