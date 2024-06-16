using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers
{
    public class ChooseNameHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseName; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            stateData.WorldSpriteName = member.PortraitName;

            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.ChoosePortrait,
                extraData:member));

            stateData.AddInputField("Name: ", delegate (string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    member.Name = text;
                    _crawlerService.GetParty().Members.Add(member);                  
                    _crawlerService.ChangeState(ECrawlerStates.TavernMain, token);              
                    _crawlerService.SaveGame();
                }
            });


            await Task.CompletedTask;
            return stateData;
        }
    }
}
