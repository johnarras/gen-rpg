
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Tavern.DeleteMember
{
    public class DeleteConfirmHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.DeleteConfirm; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            stateData.Actions.Add(new CrawlerStateAction("Delete " + member.Name + "?\n\n", CharCodes.None, ECrawlerStates.DeleteMember, null, null,
                member.PortraitName));

            stateData.Actions.Add(new CrawlerStateAction("Yes", 'Y', ECrawlerStates.DeleteMember,
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

            stateData.Actions.Add(new CrawlerStateAction("No", 'N', ECrawlerStates.DeleteMember));



            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.DeleteMember));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
