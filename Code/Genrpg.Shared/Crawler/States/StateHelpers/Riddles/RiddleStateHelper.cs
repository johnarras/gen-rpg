using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Riddles
{
    public class RiddleStateHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() {  return ECrawlerStates.Riddle; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            MapCellDetail detail = action.ExtraData as MapCellDetail;

            CrawlerStateData errorState = new CrawlerStateData(ECrawlerStates.ExploreWorld, true);

            if (string.IsNullOrEmpty(detail.Text))
            {
                return errorState;
            }

            string[] lines = detail.Text.Split("\n");

            for (int l = 0; l < lines.Length; l++)
            {
                stateData.Actions.Add(new CrawlerStateAction("\"" + lines[l] + "\""));
                stateData.Actions.Add(new CrawlerStateAction(" \n"));
            }

            stateData.Actions.Add(new CrawlerStateAction(" \n"));

            stateData.Actions.Add(new CrawlerStateAction($"\n\nPress {_textService.HighlightText("Space")} to continue...", CharCodes.Space, ECrawlerStates.ExploreWorld));
            await Task.CompletedTask;
            return stateData;
        }
    }
}
