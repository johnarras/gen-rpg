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

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guild
{
    public class GuildMainHelper : BaseStateHelper
    {
        private ITimeOfDayService _timeService = null;

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
            stateData.Actions.Add(new CrawlerStateAction("More Options", 'M', ECrawlerStates.Options));

            if (!party.InTavern)
            {
                await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.Tavern);
            }
            party.InTavern = true;
            return stateData;

        }
    }
}
