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
using UI.Screens.Constants;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Exploring
{
    public class ExploreWorldHelper : BasePartyMemberSelectHelper
    {
        private ICrawlerMapService _crawlerMapService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.ExploreWorld; }
        public override bool IsTopLevelState() { return true; }
        protected override bool ShowSelectText() { return true; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = await base.Init(currentData, action, token);

            stateData.WorldSpriteName = null;

            EnterCrawlerMapData mapData = action.ExtraData as EnterCrawlerMapData;

            PartyData party = _crawlerService.GetParty();
            party.Combat = null;

            if (mapData == null)
            {
                long worldId = _rand.Next() % 5000000;

#if UNITY_EDITOR
                if (InitClient.EditorInstance.MapGenSeed > 0)
                {
                    worldId = InitClient.EditorInstance.MapGenSeed;
                }
#endif

                CrawlerWorld world = await _worldService.GetWorld(worldId);

                party.WorldId = world.IdKey;
                stateData.Actions.Add(new CrawlerStateAction("Inn", KeyCode.I, ECrawlerStates.TavernMain));
                stateData.Actions.Add(new CrawlerStateAction("Train", KeyCode.T, ECrawlerStates.TrainingMain));
                stateData.Actions.Insert(0, new CrawlerStateAction("Cast", KeyCode.C, ECrawlerStates.SelectAlly));
                stateData.Actions.Add(new CrawlerStateAction("Vendor", KeyCode.V, ECrawlerStates.Vendor));

                stateData.Actions.Add(new CrawlerStateAction("Fight", KeyCode.F, ECrawlerStates.StartCombat));

                stateData.Actions.Add(new CrawlerStateAction("Go Adventure", KeyCode.G, ECrawlerStates.ExploreWorld, extraData: new EnterCrawlerMapData()
                {
                    MapId = 2,
                    MapX = 5,
                    MapZ = 3
                }));

                stateData.Actions.Add(new CrawlerStateAction("Back to the City", KeyCode.B, ECrawlerStates.ExploreWorld,
                    extraData: new EnterCrawlerMapData()
                    {
                        MapId = 1,
                        MapX = 13,
                        MapZ = 13,
                        MapRot = 0,
                        World = world,
                        Map = world.GetMap(1),
                    }));

                if (mapData == null)
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
            }

            await _crawlerMapService.EnterMap(party, mapData, token);

            return stateData;

        }
    }
}
