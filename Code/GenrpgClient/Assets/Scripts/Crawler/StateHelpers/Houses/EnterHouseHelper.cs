using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Training
{
    public class EnterHouseHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.EnterHouse; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();


            PartyData party = _crawlerService.GetParty();

            int index = (party.MapX * 11 + party.MapZ * 31) % 5;

            stateData.WorldSpriteName = CrawlerClientConstants.HouseImage + (index + 1);


            if (_rand.NextDouble() < 0.3f)
            {
                stateData = new CrawlerStateData(ECrawlerStates.StartCombat, true)
                {
                    WorldSpriteName = CrawlerClientConstants.HouseImage + (index + 1),
                };
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction("Exit House", KeyCode.Escape, ECrawlerStates.ExploreWorld));
                stateData.Actions.Add(new CrawlerStateAction("Exit House", KeyCode.Space, ECrawlerStates.ExploreWorld));
            }
            await Task.CompletedTask;
            return stateData;
        }
    }
}
