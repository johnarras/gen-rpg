using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.StateHelpers.PartyMembers;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using TMPro;
using UI.Screens.Constants;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Exploring
{
    public class ExploreWorldHelper : BasePartyMemberSelectHelper
    {
        private ICrawlerMapService _crawlerMapService;
        private IDispatcher _dispatcher;

        public override ECrawlerStates GetKey() { return ECrawlerStates.ExploreWorld; }
        public override bool IsTopLevelState() { return true; }
        protected override bool ShowSelectText() { return true; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            EnterCrawlerMapData mapData = action.ExtraData as EnterCrawlerMapData;

            PartyData party = _crawlerService.GetParty();
            party.Combat = null;

            if (mapData == null)
            {
                CrawlerStateData topLevelData = _crawlerService.GetTopLevelState();
                if (topLevelData != null && topLevelData.Id == ECrawlerStates.ExploreWorld)
                {
                    topLevelData.DoNotTransitionToThisState = true;
                    _dispatcher.Dispatch(topLevelData);
                    return topLevelData;
                }
            }

            CrawlerStateData stateData = await base.Init(currentData, action, token);

            stateData.WorldSpriteName = null;

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);
            stateData.Actions.Add(new CrawlerStateAction("Inn", KeyCode.I, ECrawlerStates.TavernMain));
            stateData.Actions.Add(new CrawlerStateAction("Train", KeyCode.T, ECrawlerStates.TrainingMain));
            stateData.Actions.Insert(0, new CrawlerStateAction("Cast", KeyCode.C, ECrawlerStates.SelectAlly));
            stateData.Actions.Add(new CrawlerStateAction("Vendor", KeyCode.V, ECrawlerStates.Vendor));

            stateData.Actions.Add(new CrawlerStateAction("Fight", KeyCode.F, ECrawlerStates.StartCombat));

            CrawlerMap firstCity = world.GetMap(2);

            EnterCrawlerMapData firstCityData = new EnterCrawlerMapData()
            {
                MapId = 2,
                MapX = firstCity.Width / 2,
                MapZ = firstCity.Height / 2,
                MapRot = 0,
                World = world,
                Map = firstCity,
            };

            stateData.Actions.Add(new CrawlerStateAction("Outdoors", KeyCode.O, ECrawlerStates.ExploreWorld,
                extraData: firstCityData));

            if (mapData == null)
            {
                if (world.GetMap(party.MapId) != null)
                {
                    mapData = new EnterCrawlerMapData()
                    {
                        MapId = party.MapId,
                        MapX = party.MapX,
                        MapZ = party.MapZ,
                        MapRot = party.MapRot,
                        World = world,
                        Map = world.GetMap(party.MapId),
                    };
                }
                else
                {
                    mapData = firstCityData;
                }
            }

            await _crawlerMapService.EnterMap(party, mapData, token);

            return stateData;
        }
    }
}
