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
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Stats.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class MapExitHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.MapExit; }
        public override long TriggerDetailEntityTypeId() { return EntityTypes.Map; }

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

            bool didComplete = partyData.CompletedMaps.HasBit(detail.EntityId);

            if (partyMap == null && !didComplete)
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

                        stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));

                        return stateData;
                    }
                }

                _logService.Info("UnlockText: " + map.RiddleText + " Answer: " + map.RiddleAnswer);
                if (!string.IsNullOrEmpty(map.RiddleText) && !string.IsNullOrEmpty(map.RiddleAnswer))
                {
                    string[] descLines = map.RiddleText.Split("\n");

                    stateData.Actions.Add(new CrawlerStateAction("Answer this to pass:\n"));
                    stateData.Actions.Add(new CrawlerStateAction(" "));
                    for (int d = 0; d < descLines.Length; d++)
                    {
                        if (!string.IsNullOrEmpty(descLines[d]))
                        {
                            stateData.Actions.Add(new CrawlerStateAction(descLines[d].Substring(0, (int)MathUtils.Min(descLines[d].Length, 6)) + "..."));
                        }
                    }

                    if (string.IsNullOrEmpty(errorText))
                    {
                        stateData.Actions.Add(new CrawlerStateAction(" "));
                    }
                    else
                    {
                        stateData.Actions.Add(new CrawlerStateAction(_textService.HighlightText(errorText, TextColors.ColorRed)));
                    }

                    stateData.AddInputField("Answer:", delegate (string text)
                    {
                        string normalizedRiddleName = map.RiddleAnswer.ToLower().Trim();

                        string normalizedText = text.ToLower().Trim();

                        normalizedText = new string (text.Where(char.IsLetterOrDigit).ToArray()).ToLower();

                        if (!string.IsNullOrEmpty(normalizedText) && normalizedText == normalizedRiddleName)
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
                                ErrorText = map.RiddleError,
                            };

                            foreach (PartyMember member in partyData.GetActiveParty())
                            {
                                member.Stats.SetCurr(StatTypes.Health, member.Stats.Curr(StatTypes.Health) / 2);
                            }
                            _crawlerService.ChangeState(ECrawlerStates.MapExit, token, newErrorDetail);
                        }
                    });
                    stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));

                    return stateData;
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
