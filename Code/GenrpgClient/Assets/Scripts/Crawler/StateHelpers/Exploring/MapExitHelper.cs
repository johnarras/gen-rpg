using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.UI.Utils;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Exploring
{
    public class MapExitHelper : BaseStateHelper
    {
        private ICrawlerMapService _crawlerMapService;


        public override ECrawlerStates GetKey() { return ECrawlerStates.MapExit; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            MapCellDetail detail = action.ExtraData as MapCellDetail;

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

            if (map.QuestItemsNeeded.Count > 0)
            {
                List<WorldQuestItem> itemsNeeded = new List<WorldQuestItem>();

                foreach (MapQuestItem mqi in map.QuestItemsNeeded)
                {
                    PartyQuestItem pqi = partyData.QuestItems.FirstOrDefault(x => x.CrawlerQuestItemId == mqi.QuestItemId);

                    if (pqi == null)
                    {
                        WorldQuestItem wqi = world.QuestItems.FirstOrDefault(x => x.IdKey == mqi.QuestItemId);
                        if (wqi != null)
                        {
                            itemsNeeded.Add(wqi);
                        }
                    }
                }

                if (itemsNeeded.Count > 1)
                {
                    stateData.Actions.Add(new CrawlerStateAction(map.Name + " requires the following to enter: "));

                    foreach (WorldQuestItem wqi in itemsNeeded) 
                    {
                        stateData.Actions.Add(new CrawlerStateAction(wqi.Name));
                    }

                    stateData.Actions.Add(new CrawlerStateAction($"\n\nPress {CrawlerUIUtils.HighlightText("Space")} to continue...", KeyCode.Space, ECrawlerStates.ExploreWorld));

                    return stateData;
                }
            }

            stateData.Actions.Add(new CrawlerStateAction("Go to " + map.GetName(detail.ToX, detail.ToZ) + "?\n\n", KeyCode.None, ECrawlerStates.None, null, null));

            stateData.Actions.Add(new CrawlerStateAction("Yes", KeyCode.Y, ECrawlerStates.ExploreWorld, null,
               new EnterCrawlerMapData()
               {
                   MapId = map.IdKey,
                   MapX = detail.ToX,
                   MapZ = detail.ToZ,
                   MapRot = 0,
                   World = world,
                   Map = map,
               }));

            stateData.Actions.Add(new CrawlerStateAction("No", KeyCode.N, ECrawlerStates.ExploreWorld));
               

            await Task.CompletedTask;
            return stateData;
        }
    }
}
