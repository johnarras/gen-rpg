using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Units.Entities;
using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Combat
{
    public class CombatFightRunStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.CombatFightRun; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData partyData = _crawlerService.GetParty();

            if (partyData.Combat == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Party is not in combat." };
            }

            bool didShowPortrait = false;
            stateData.Actions.Add(new CrawlerStateAction("You face: "));

            foreach (CombatGroup group in partyData.Combat.Enemies)
            {
                if (group.Units.Count < 1)
                {
                    continue;
                }

                if (!didShowPortrait)
                {
                    partyData.WorldPanel.SetPicture(group.Units[0].PortraitName);
                    stateData.WorldSpriteName = group.Units[0].PortraitName;
                    didShowPortrait = true;
                }

                stateData.Actions.Add(new CrawlerStateAction(group.ShowStatus()));
            }

            stateData.Actions.Add(new CrawlerStateAction(" "));

            stateData.Actions.Add(new CrawlerStateAction("Fight", KeyCode.F, ECrawlerStates.CombatPlayer,
                   onClickAction: delegate ()
                   {
                       partyData.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Fight;
                   }));



            stateData.Actions.Add(new CrawlerStateAction("Run", KeyCode.R, ECrawlerStates.CombatConfirm,
                onClickAction: delegate ()
                {
                    partyData.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Run;
                }));


            long minRange = CrawlerCombatConstants.MaxRange;

            foreach (CombatGroup group in partyData.Combat.Enemies)
            {
                minRange = Math.Min(minRange, group.Range);
            }

            if (minRange > CrawlerCombatConstants.MinRange)
            {
                stateData.Actions.Add(new CrawlerStateAction("Advance", KeyCode.A, ECrawlerStates.CombatConfirm,
               onClickAction: delegate ()
               {
                   partyData.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Advance;
               }));
            }


            await Task.CompletedTask;
            return stateData;
        }
    }
}
