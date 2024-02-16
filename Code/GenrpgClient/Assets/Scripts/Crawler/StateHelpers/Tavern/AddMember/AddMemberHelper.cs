using Assets.Scripts.Atlas.Constants;
using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Parties.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Tavern.AddMember
{
    public class AddMemberHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.AddMember; }

        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData partyData = _crawlerService.GetParty();

            for (int m = 0; m < partyData.Members.Count; m++)
            {
                PartyMember member = partyData.Members[m];
                char c = (char)('a' + m);

                KeyCode kc = (KeyCode)c;
                if (member.PartySlot > 0)
                {
                    kc = KeyCode.None;
                }
                stateData.Actions.Add(new CrawlerStateAction(char.ToUpper(c) + " " + member.Name, kc, ECrawlerStates.AddMember,
                    delegate
                    {
                        if (member.PartySlot > 0)
                        {
                            return;
                        }

                        partyData = _crawlerService.GetParty();

                        partyData.AddPartyMember(member);
                        _statService.CalcPartyStats(gs, partyData, true);
                        _crawlerService.SaveGame();


                    }, member, member.PortraitName));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.TavernMain, null, null));

            await UniTask.CompletedTask;
            return stateData;
        }
    }
}
