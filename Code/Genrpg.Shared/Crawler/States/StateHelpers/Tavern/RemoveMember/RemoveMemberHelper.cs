
using Genrpg.Shared.Crawler.Parties.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Tavern.RemoveMember
{
    public class RemoveMemberHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.RemoveMember; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData partyData = _crawlerService.GetParty();

            for (int m = 0; m < partyData.Members.Count; m++)
            {
                PartyMember member = partyData.Members[m];
                char c = (char)('a' + m);

                if (member.PartySlot == 0)
                {
                    c = CharCodes.None;
                }
                stateData.Actions.Add(new CrawlerStateAction(char.ToUpper(c) + " " + member.Name, c, ECrawlerStates.RemoveMember,
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

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.TavernMain, null, null));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
