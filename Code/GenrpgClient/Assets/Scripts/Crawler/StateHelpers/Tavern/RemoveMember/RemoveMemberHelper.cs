using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;

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
    public class RemoveMemberHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.RemoveMember; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData partyData = _crawlerService.GetParty();

            for (int m = 0; m < partyData.Members.Count; m++)
            {
                PartyMember member = partyData.Members[m];
                char c = (char)('a' + m);

                KeyCode kc = (KeyCode)c;
                if (member.PartySlot == 0)
                {
                    kc = KeyCode.None;
                }
                stateData.Actions.Add(new CrawlerStateAction(char.ToUpper(c) + " " + member.Name, kc, ECrawlerStates.RemoveMember,
                    delegate
                    {
                        if (member.PartySlot < 1)
                        {
                            return;
                        }
                        
                       
                        partyData.RemovePartyMember(member);
                        _statService.CalcPartyStats(partyData, true);
                        _crawlerService.SaveGame();

                    }, member));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.TavernMain, null, null));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
