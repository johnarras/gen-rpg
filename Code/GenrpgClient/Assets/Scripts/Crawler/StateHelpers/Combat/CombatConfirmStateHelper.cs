﻿using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Combat
{
    public class CombatConfirmStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.CombatConfirm; }

        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            if (party.Combat == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Party is not in combat." };
            }

            if (party.Combat.Allies[0].CombatGroupAction == ECombatGroupActions.Advance)
            {
                stateData.Actions.Add(new CrawlerStateAction("Are you sure you wish to advance?", KeyCode.None, ECrawlerStates.None));
            }
            else if (party.Combat.Allies[0].CombatGroupAction == ECombatGroupActions.Run)
            {
                stateData.Actions.Add(new CrawlerStateAction("Are you sure you wish to run?", KeyCode.None, ECrawlerStates.None));
            }
            else
            {
                foreach (CrawlerUnit combatUnit in party.Combat.Allies[0].Units)
                {
                    stateData.Actions.Add(new CrawlerStateAction(combatUnit.Name + ": " + combatUnit.Action.Text));
                }
                stateData.Actions.Add(new CrawlerStateAction("Use these actions?", KeyCode.None, ECrawlerStates.None));
            }

            stateData.Actions.Add(new CrawlerStateAction("Yes", KeyCode.Y, ECrawlerStates.ProcessCombatRound));
            stateData.Actions.Add(new CrawlerStateAction("No", KeyCode.N, ECrawlerStates.CombatFightRun,
                onClickAction: delegate ()
                {
                    // Need to reset all combat round data and start over.
                    _combatService.EndCombatRound(gs, party);
                }));

            await UniTask.CompletedTask;
            return stateData;


        }
    }
}