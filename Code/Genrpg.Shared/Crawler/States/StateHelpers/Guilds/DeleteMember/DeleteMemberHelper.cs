using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Guild.DeleteMember
{
    public class DeleteMemberHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.DeleteMember; }

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
                stateData.Actions.Add(new CrawlerStateAction(member.Name, CharCodes.None, ECrawlerStates.DeleteConfirm, null,
                    member, member.PortraitName));

            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.GuildMain, null, null));

            await Task.CompletedTask;

            return stateData;
        }
    }
}
