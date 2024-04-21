using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Factions.Settings;
using Genrpg.Shared.Spells.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Combat
{
    public class CombatPlayerStateHelper : BaseCombatStateHelper
    {

        public override ECrawlerStates GetKey() { return ECrawlerStates.CombatPlayer; }

        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {

            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            if (party.Combat == null || party.Combat.Allies.Count < 1 ||
                party.Combat.PartyGroup.CombatGroupAction != ECombatGroupActions.Fight)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Party is not fighting in combat" };
            }

            CombatGroup group = party.Combat.PartyGroup;

            List<CrawlerUnit> readyUnits = group.Units.Where(x => x is PartyMember member && x.Action != null).ToList();

            List<CrawlerUnit> notReadyUnits = group.Units.Where(x => x is PartyMember member && x.Action == null).ToList();


            if (notReadyUnits.Count < 1)
            {
                return new CrawlerStateData(ECrawlerStates.CombatConfirm, true);
            }

            if (readyUnits.Count == 0)
            {
                stateData.Actions.Add(new CrawlerStateAction("", KeyCode.Escape, ECrawlerStates.CombatFightRun,
                    onClickAction: delegate ()
                    {
                        group.CombatGroupAction = ECombatGroupActions.None;
                    }));
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction("", KeyCode.Escape, ECrawlerStates.CombatPlayer,
                    onClickAction: delegate ()
                    {
                        readyUnits.Last().Action = null;
                    }));
            }

            stateData.WorldSpriteName = notReadyUnits[0].PortraitName;
            List<UnitAction> actions = _combatService.GetActionsForPlayer(gs, party, notReadyUnits[0]);

            if (actions.Count == 1 && actions[0].CombatActionId == CombatActions.Disabled)
            {
                notReadyUnits[0].Action = actions[0];
                return new CrawlerStateData(ECrawlerStates.CombatPlayer, true);
            }

            PartyMember notReadyMember = notReadyUnits[0] as PartyMember;

            List<KeyCode> usedKeyCodes = new List<KeyCode>();

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
                        return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Bad target list for combat spell" };
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

            await UniTask.CompletedTask;
            return stateData;
        }

        private KeyCode GetKeyCode(List<KeyCode> usedKeyCodes, CombatAction combatAction,  UnitAction unitAction)
        {

            KeyCode newKeyCode = (KeyCode)(char.ToLower(combatAction.Name[0]));
            if (usedKeyCodes.Contains(newKeyCode))
            {
                newKeyCode = (KeyCode)(char.ToLower(unitAction.Spell.Name[0]));

                if (!usedKeyCodes.Contains(newKeyCode))
                {
                    usedKeyCodes.Add(newKeyCode);
                }
            }
            else
            {
                usedKeyCodes.Add(newKeyCode);
            }
            return newKeyCode;
        }
    }
}
