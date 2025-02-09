using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.Stats.Constants;
using System.Threading;
using Genrpg.Shared.Crawler.Maps.Services;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds
{
    public class GuildMainHelper : BuildingStateHelper
    {
        private ITimeOfDayService _timeService = null;
        private ICrawlerMapService _mapService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.GuildMain; }
        public override long TriggerBuildingId() { return BuildingTypes.Guild; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.WorldSpriteName = CrawlerClientConstants.TavernImage;

            PartyData party = _crawlerService.GetParty();

            foreach (PartyMember member in party.Members)
            {
                member.Stats.SetCurr(StatTypes.Health, member.Stats.Max(StatTypes.Health));
                member.Stats.SetCurr(StatTypes.Mana, member.Stats.Max(StatTypes.Mana));
                member.StatusEffects.Clear();
            }

            string txt = action.ExtraData as string;

            if (txt != null && txt == "GenerateWorld")
            {
                CrawlerMap map = _worldService.GetMap(party.MapId);

                if (map == null || map.CrawlerMapTypeId == CrawlerMapTypes.City)
                {
                    await _worldService.GenerateWorld(party);
                    _mapService.CleanMap();
                }
            }

            stateData.Actions.Add(new CrawlerStateAction("Add Char", 'A', ECrawlerStates.AddMember));
            stateData.Actions.Add(new CrawlerStateAction("Remove Char", 'R', ECrawlerStates.RemoveMember));
            stateData.Actions.Add(new CrawlerStateAction("Delete Char", 'D', ECrawlerStates.DeleteMember));
            stateData.Actions.Add(new CrawlerStateAction("Create Char", 'C', ECrawlerStates.ChooseSex));
            stateData.Actions.Add(new CrawlerStateAction("New Game", 'N', ECrawlerStates.GuildMain, null, "GenerateWorld"));

            if (party.GetActiveParty().Count > 0)
            {
                stateData.Actions.Add(new CrawlerStateAction("Enter Map", 'E', ECrawlerStates.ExploreWorld));
            }
            if (party.GameMode == ECrawlerGameModes.Roguelite)
            {
                stateData.Actions.Add(new CrawlerStateAction("Upgrades", 'U', ECrawlerStates.UpgradeParty));
            }
            //stateData.Actions.Add(new CrawlerStateAction("More Options", 'M', ECrawlerStates.Options));

            if (!party.InGuildHall)
            {
                await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.Tavern);
            }
            party.InGuildHall = true;
            return stateData;

        }
    }
}
