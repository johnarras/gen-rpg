using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Guild.AddMember
{
    public class AddMemberHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.AddMember; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData partyData = _crawlerService.GetParty();

            for (int m = 0; m < partyData.Members.Count; m++)
            {
                PartyMember member = partyData.Members[m];

                if (member.PartySlot > 0)
                {
                    continue;
                }
                stateData.Actions.Add(new CrawlerStateAction(member.Name, CharCodes.None, ECrawlerStates.AddMember,
                    delegate
                    {
                        if (member.PartySlot > 0)
                        {
                            return;
                        }

                        partyData = _crawlerService.GetParty();

                        partyData.AddPartyMember(member);
                        _statService.CalcPartyStats(partyData, true);
                        _crawlerService.SaveGame();


                    }, member, member.PortraitName));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.GuildMain, null, null));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
