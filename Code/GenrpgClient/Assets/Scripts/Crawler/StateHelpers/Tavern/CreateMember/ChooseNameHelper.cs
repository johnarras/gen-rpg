using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers
{
    public class ChooseNameHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseName; }

        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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

            await UniTask.CompletedTask;
            return stateData;
        }
    }
}
