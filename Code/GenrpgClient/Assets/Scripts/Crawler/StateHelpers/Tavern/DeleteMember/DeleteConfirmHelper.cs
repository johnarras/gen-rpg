using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Tavern.DeleteMember
{
    public class DeleteConfirmHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() {  return ECrawlerStates.DeleteConfirm; }

        public override async UniTask<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {

            

            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            stateData.Actions.Add(new CrawlerStateAction("Delete " + member.Name + "?\n\n", KeyCode.None, ECrawlerStates.DeleteMember, null, null,
                member.PortraitName));

            stateData.Actions.Add(new CrawlerStateAction("Yes", KeyCode.Y, ECrawlerStates.DeleteMember, 
                delegate
                {
                    if (member.PartySlot > 0)
                    {
                        return;
                    }

                    PartyData partyData = _crawlerService.GetParty();

                    partyData.DeletePartyMember(member);

                    _crawlerService.SaveGame();

                }));

            stateData.Actions.Add(new CrawlerStateAction("No", KeyCode.N, ECrawlerStates.DeleteMember));



            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.DeleteMember));
            await UniTask.CompletedTask;
            return stateData;
        }
    }
}
