using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers.Combat;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Selection
{
    public class OnSelectSpellStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.OnSelectSpell; }

        public override async UniTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            SelectSpellAction selectSpell = action.ExtraData as SelectSpellAction;

            if (selectSpell == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Select Spell Action missing on select" };
            }

            PartyData party = _crawlerService.GetParty();

            UnitAction newAction = _combatService.GetActionFromSpell(party, selectSpell.Action.Member,
                selectSpell.Spell, null);

            if (newAction == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Failed to create action after selecting spell" };
            }

            selectSpell.Action.Action = newAction;
            selectSpell.Action.Member.Action = newAction;

            if (selectSpell.Spell.TargetTypeId == TargetTypes.Special)
            {
                return new CrawlerStateData(ECrawlerStates.SpecialSpellCast, true) { ExtraData = selectSpell };
            }

            ECrawlerStates nextState = selectSpell.Action.NextState;
            if (newAction.FinalTargets.Count < 1)
            {
                if (newAction.PossibleTargetGroups.Count > 0)
                {
                    return new CrawlerStateData(ECrawlerStates.SelectEnemyGroup, true) { ExtraData = selectSpell };
                }
                else if (newAction.PossibleTargetUnits.Count > 0)
                {
                    return new CrawlerStateData(ECrawlerStates.SelectAllyTarget, true) { ExtraData = selectSpell };
                }
                else
                {
                    return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Selected spell but no targets available" };
                }
            }

            await UniTask.CompletedTask;
            return new CrawlerStateData(nextState,true) {  ExtraData = selectSpell };
        }
    }
}
