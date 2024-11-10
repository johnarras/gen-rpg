using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Temples.Services;
using Genrpg.Shared.Crawler.Training.Services;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Temple
{
    public class TempleStateHelper : BaseStateHelper
    {

        private ITempleService _templeService = null;

        public override ECrawlerStates GetKey() { return ECrawlerStates.Temple; }
        public override long TriggerBuildingId() { return BuildingTypes.Temple; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData partyData = _crawlerService.GetParty();

            TempleResult result = action.ExtraData as TempleResult;

            if (result != null)
            {
                string color = result.Success ? TextColors.ColorYellow : TextColors.ColorRed;

                stateData.Actions.Add(new CrawlerStateAction(_textService.HighlightText(result.Message,color)));
            }

            stateData.Actions.Add(new CrawlerStateAction("Party Gold: " + partyData.Gold));

            foreach (PartyMember member in partyData.GetActiveParty())
            {
                long cost = _templeService.GetHealingCostForMember(partyData, member);
                if (cost > 0)
                {
                    TempleResult newResult = new TempleResult();
                    stateData.Actions.Add(new CrawlerStateAction(member.Name + "(" + cost + ")", CharCodes.None, ECrawlerStates.Temple,
                        () =>
                        {
                            _templeService.HealPartyMember(partyData, member, newResult);
                        }, forceButton:true, extraData: newResult));
                }
            }


            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));
            await Task.CompletedTask;
            return stateData;

        }
    }
}
