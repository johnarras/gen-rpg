using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
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
                return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Missing map at this coordinate." };
            }

            PartyData partyData = _crawlerService.GetParty();

            CrawlerWorld world = await _worldService.GetWorld(partyData.WorldId);

            CrawlerMap map = world.GetMap(detail.EntityId);

            if (map == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "No such map exists." };
            }


            stateData.Actions.Add(new CrawlerStateAction("Go to " + map.IdKey + "?\n\n", KeyCode.None, ECrawlerStates.None, null, null));

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

            stateData.Actions.Add(new CrawlerStateAction("No", KeyCode.N, ECrawlerStates.ExploreWorld, null,
                 new EnterCrawlerMapData()
                 {
                     MapId = partyData.MapId,
                     MapX = partyData.MapX,
                     MapZ = partyData.MapZ,
                     MapRot = partyData.MapRot,
                     World = world,
                     Map = world.GetMap(partyData.MapId),
                 }));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
