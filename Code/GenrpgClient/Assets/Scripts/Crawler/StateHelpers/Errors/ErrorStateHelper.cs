using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.UI.Utils;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Errors
{
    public class ErrorStateHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.Error; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            party.Combat = null;

            stateData.Actions.Add(new CrawlerStateAction("An error occurred. Returning you to the main map."));

            if (!string.IsNullOrEmpty(currentData.ErrorMessage))
            {
                stateData.Actions.Add(new CrawlerStateAction("\n" + currentData.ErrorMessage + "\n"));
            }

            stateData.Actions.Add(new CrawlerStateAction($"\n\nPress {CrawlerUIUtils.HighlightText("Space")} to continue...", KeyCode.Space, ECrawlerStates.ExploreWorld));

            
            return stateData;
        }
    }
}
