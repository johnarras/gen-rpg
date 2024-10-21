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

            if (detail == null || detail.EntityTypeId != EntityTypes.Riddle)
            {
                return errorState;
            }

            Riddle riddle = _gameData.Get<RiddleSettings>(_gs.ch).Get(detail.EntityId);

            if (riddle == null || string.IsNullOrEmpty(riddle.Desc) || string.IsNullOrEmpty(riddle.Name))
            {
                return errorState;
            }

            string[] lines = riddle.Desc.Split('\n');   

            if (detail.Index < 0 || detail.Index >= lines.Length)
            {
                return errorState;
            }

            stateData.Actions.Add(new CrawlerStateAction("Some strange writing is on the wall..."));


            stateData.Actions.Add(new CrawlerStateAction("\n"));
            stateData.Actions.Add(new CrawlerStateAction(lines[(int)detail.Index]));

            stateData.Actions.Add(new CrawlerStateAction("\n"));

            int letterIndex = (int)(detail.Index*31 + riddle.IdKey * 7) % riddle.Name.Length;

            StringBuilder answer = new StringBuilder();

            for (int i = 0; i < riddle.Name.Length; i++)
            {
                if (i !=  letterIndex)
                {
                    answer.Append("_");
                }
                else
                {
                    answer.Append(char.ToUpper(riddle.Name[i]));
                }
            }

            stateData.Actions.Add(new CrawlerStateAction(answer.ToString()));


            stateData.Actions.Add(new CrawlerStateAction("\n"));

            stateData.Actions.Add(new CrawlerStateAction($"\n\nPress {_textService.HighlightText("Space")} to continue...", CharCodes.Space, ECrawlerStates.ExploreWorld));

            return stateData;
        }
    }
}
