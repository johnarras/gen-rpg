using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Genrpg.Shared.Crawler.Maps.Services;
using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class ExploreWorldHelper : BaseStateHelper
    {

        public class NamedMoveKey
        {
            public char Key { get; private set; }
            public string Name { get; private set; }

            public NamedMoveKey(char key, string name)
            {
                Key = key;
                Name = name;
            }
        }

        private ICrawlerMapService _crawlerMapService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.ExploreWorld; }
        public override bool IsTopLevelState() { return true; }

        protected static readonly NamedMoveKey[] _moveKeys = new NamedMoveKey[]{
                new NamedMoveKey('Q', "Strafe Left"),
                new NamedMoveKey('W', "Forward"),
                new NamedMoveKey('E', "Strafe Right"),
                new NamedMoveKey('A', "Turn Left"),
                new NamedMoveKey('S', "Back"),
                new NamedMoveKey('D', "Turn Right"),
        };

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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

            CrawlerStateData stateData = CreateStateData();

            stateData.WorldSpriteName = null;

            List<PartyMember> members = party.GetActiveParty();

            int maxPartySlot = 0;
            if (members.Count > 0)
            {
                maxPartySlot = members.Max(x => x.PartySlot);
            }
            if (maxPartySlot > 0)
            {
                stateData.Actions.Add(new CrawlerStateAction("1- " + maxPartySlot + " or the status panel below to view members", CharCodes.None, ECrawlerStates.None));
                foreach (PartyMember member in members)
                {
                    stateData.Actions.Add(new CrawlerStateAction(member.Name, (char)(member.PartySlot + '0'),
                        ECrawlerStates.PartyMember, extraData: member));

                }
            }

            stateData.Actions.Add(new CrawlerStateAction(null, rowFiller: true));


            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);
            stateData.Actions.Add(new CrawlerStateAction("Cast", 'C', ECrawlerStates.SelectAlly));
            stateData.Actions.Add(new CrawlerStateAction("Inn", 'I', ECrawlerStates.TavernMain));
            stateData.Actions.Add(new CrawlerStateAction("Train", 'T', ECrawlerStates.TrainingMain));
            stateData.Actions.Add(new CrawlerStateAction("Vendor", 'V', ECrawlerStates.Vendor));
            stateData.Actions.Add(new CrawlerStateAction("Fight", 'F', ECrawlerStates.StartCombat));

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

            stateData.Actions.Add(new CrawlerStateAction("Outdoors", 'O', ECrawlerStates.ExploreWorld,
                extraData: firstCityData));


            stateData.Actions.Add(new CrawlerStateAction(null, rowFiller: true));
            int moveKeysShown = 0;
            foreach (NamedMoveKey nmk in _moveKeys)
            {
                stateData.Actions.Add(new CrawlerStateAction(nmk.Name, nmk.Key, ECrawlerStates.DoNotChangeState, () =>
                {
                    _crawlerMapService.AddKeyInput(nmk.Key, token);
                }));
                if (++moveKeysShown % 3 == 0)
                {
                    stateData.Actions.Add(new CrawlerStateAction(null, rowFiller: true));
                }
            }


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
