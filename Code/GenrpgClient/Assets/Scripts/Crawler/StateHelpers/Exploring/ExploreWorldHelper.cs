using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.StateHelpers.PartyMembers;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using UI.Screens.Constants;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Exploring
{
    public class ExploreWorldHelper : BasePartyMemberSelectHelper
    {
        private ICrawlerMapService _crawlerMapService;
        public override ECrawlerStates GetKey() { return ECrawlerStates.ExploreWorld; }
        public override bool IsTopLevelState() { return true; }
        protected override bool ShowSelectText() { return true; }

        public override async UniTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = await base.Init(currentData, action, token);

            PartyData party = _crawlerService.GetParty();
            party.Combat = null;

            EnterCrawlerMapData mapData = action.ExtraData as EnterCrawlerMapData;

            stateData.Actions.Add(new CrawlerStateAction("Inn", KeyCode.I, ECrawlerStates.TavernMain));
            stateData.Actions.Add(new CrawlerStateAction("Train", KeyCode.T, ECrawlerStates.TrainingMain));
            stateData.Actions.Insert(0, new CrawlerStateAction("Cast", KeyCode.C, ECrawlerStates.SelectAlly));
            stateData.Actions.Add(new CrawlerStateAction("Vendor", KeyCode.V, ECrawlerStates.Vendor));

            stateData.Actions.Add(new CrawlerStateAction("Fight", KeyCode.F, ECrawlerStates.StartCombat));

            stateData.Actions.Add(new CrawlerStateAction("Go Adventure", KeyCode.G, ECrawlerStates.ExploreWorld, extraData: new EnterCrawlerMapData() { MapId=2, 
            MapX = 5, MapZ = 3}));

            stateData.Actions.Add(new CrawlerStateAction("Back to the City", KeyCode.B, ECrawlerStates.ExploreWorld,
                extraData: new EnterCrawlerMapData()
                {
                    MapId = 1,
                    MapX = 13,
                    MapZ = 13,
                    MapRot = 0,
                }));

            if (mapData == null)
            {
                mapData = new EnterCrawlerMapData()
                {
                    MapId = party.MapId,
                    MapX = party.MapX,
                    MapZ = party.MapZ,
                    MapRot = party.MapRot,
                };
            }

            await _crawlerMapService.EnterMap(party, mapData, token);

            return stateData;

        }
    }
}
