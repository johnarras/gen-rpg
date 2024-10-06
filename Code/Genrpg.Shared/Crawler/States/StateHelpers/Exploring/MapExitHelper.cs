using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Settings;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.UI.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class MapExitHelper : BaseStateHelper
    {
        private ICrawlerMapService _crawlerMapService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.MapExit; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            string errorText = null;
            MapCellDetail detail = action.ExtraData as MapCellDetail;

            ErrorMapCellDetail errorDetail = action.ExtraData as ErrorMapCellDetail;

            if (errorDetail != null)
            {
                detail = errorDetail.Detail;
                errorText = errorDetail.ErrorText;
            }

            if (detail == null || detail.EntityTypeId != EntityTypes.Map)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Missing map at this coordinate." };
            }

            PartyData partyData = _crawlerService.GetParty();

            CrawlerWorld world = await _worldService.GetWorld(partyData.WorldId);

            CrawlerMap map = world.GetMap(detail.EntityId);

            if (map == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "No such map exists." };
            }

            CrawlerMapStatus partyMap = partyData.Maps.FirstOrDefault(x => x.MapId == detail.EntityId);

            if (partyMap == null)
            {
                if (map.MapQuestItemId > 0)
                {

                    WorldQuestItem itemNeeded = null;

                    PartyQuestItem pqi = partyData.QuestItems.FirstOrDefault(x => x.CrawlerQuestItemId == map.MapQuestItemId);

                    if (pqi == null)
                    {
                        WorldQuestItem wqi = world.QuestItems.FirstOrDefault(x => x.IdKey == map.MapQuestItemId);
                        if (wqi != null)
                        {
                            itemNeeded = wqi;
                        }
                    }


                    if (itemNeeded != null)
                    {
                        stateData.Actions.Add(new CrawlerStateAction(map.Name + " requires the following to enter: "));

                        stateData.Actions.Add(new CrawlerStateAction(itemNeeded.Name));

                        stateData.Actions.Add(new CrawlerStateAction($"\n\nPress {_textService.HighlightText("Space")} to continue...", CharCodes.Space, ECrawlerStates.ExploreWorld));

                        return stateData;
                    }

                    if (map.RiddleId < 0)
                    {
                        Riddle riddle = _gameData.Get<RiddleSettings>(_gs.ch).Get(map.RiddleId);
                        if (riddle != null && !string.IsNullOrEmpty(riddle.Desc) && !string.IsNullOrEmpty(riddle.Name))
                        {
                            string[] descLines = riddle.Desc.Split('\n');

                            stateData.Actions.Add(new CrawlerStateAction("Answer me this to enter:\n"));
                            for (int d = 0; d < descLines.Length; d++)
                            {
                                stateData.Actions.Add(new CrawlerStateAction(descLines[d]));
                            }

                            if (string.IsNullOrEmpty(errorText))
                            {
                                stateData.Actions.Add(new CrawlerStateAction(" "));
                            }
                            else
                            {
                                stateData.Actions.Add(new CrawlerStateAction(_textService.HighlightText(errorText, TextColors.ColorRed)));
                            }

                            stateData.AddInputField("Your Answer:\n", delegate (string text)
                            {
                                string normalizedRiddleName = riddle.Name.ToLower().Trim();
                                if (!string.IsNullOrEmpty(text) && text.ToLower().Trim() == normalizedRiddleName)
                                {
                                    EnterCrawlerMapData enterMapData = new EnterCrawlerMapData()
                                    {
                                        MapId = map.IdKey,
                                        MapX = detail.ToX,
                                        MapZ = detail.ToZ,
                                        MapRot = 0,
                                        World = world,
                                        Map = map,
                                    };

                                    _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token, enterMapData);
                                }
                                else
                                {
                                    ErrorMapCellDetail newErrorDetail = new ErrorMapCellDetail()
                                    {
                                        Detail = detail,
                                        ErrorText = "Sorry, that is not correct! Search for clues nearby and try again.",
                                    };
                                    _crawlerService.ChangeState(ECrawlerStates.MapExit, token, newErrorDetail);
                                }
                            });
                        }
                    }
                }
            }

            stateData.Actions.Add(new CrawlerStateAction("Go to " + map.GetName(detail.ToX, detail.ToZ) + "?\n\n", CharCodes.None, ECrawlerStates.None, null, null));

            stateData.Actions.Add(new CrawlerStateAction("Yes", 'Y', ECrawlerStates.ExploreWorld, null,
               new EnterCrawlerMapData()
               {
                   MapId = map.IdKey,
                   MapX = detail.ToX,
                   MapZ = detail.ToZ,
                   MapRot = 0,
                   World = world,
                   Map = map,
               }));

            stateData.Actions.Add(new CrawlerStateAction("No", 'N', ECrawlerStates.ExploreWorld));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
