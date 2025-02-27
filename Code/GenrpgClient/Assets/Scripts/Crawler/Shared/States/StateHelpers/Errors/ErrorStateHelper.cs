
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Errors
{
    public class ErrorStateHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.Error; }



        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            _combatService.EndCombat(party);

            stateData.Actions.Add(new CrawlerStateAction("An error occurred.\nReturning you to the main map."));

            if (currentData.ExtraData != null && !string.IsNullOrEmpty(currentData.ExtraData.ToString()))
            {
                stateData.Actions.Add(new CrawlerStateAction(_textService.HighlightText("\n" + currentData.ExtraData.ToString() + "\n", TextColors.ColorRed)));
            }

            AddSpaceAction(stateData);
          
            await Task.CompletedTask;
            return stateData;
        }
    }
}
