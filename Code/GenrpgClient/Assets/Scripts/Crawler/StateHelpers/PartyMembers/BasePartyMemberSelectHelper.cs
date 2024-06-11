using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.PartyMembers
{
    public abstract class BasePartyMemberSelectHelper : BaseStateHelper
    {

        protected virtual bool ShowSelectText() { return false; }
        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData partyData = _crawlerService.GetParty();

            List<PartyMember> members = partyData.Members.Where(x => x.PartySlot > 0).ToList();

            int maxPartySlot = 0;
            if (members.Count > 0)
            {
                maxPartySlot = members.Max(x => x.PartySlot);
            }
            if (maxPartySlot > 0)
            {
                if (ShowSelectText())
                {
                    stateData.Actions.Add(new CrawlerStateAction("1- " + maxPartySlot + " to view party members", KeyCode.None, ECrawlerStates.None));
                }

                foreach (PartyMember member in members)
                {
                    stateData.Actions.Add(new CrawlerStateAction(null, (KeyCode)(member.PartySlot + '0'),
                        ECrawlerStates.PartyMember, extraData: member));

                }
            }

            
            return stateData;
        }
    }
}
