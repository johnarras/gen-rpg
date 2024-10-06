using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class PartyMemberHelper : BaseStateHelper
    {
        private IScreenService _screenService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.PartyMember; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {

            PartyData partyData = _crawlerService.GetParty();
            PartyMember member = action.ExtraData as PartyMember;

            CrawlerStateData crawlerStateData = CreateStateData();
            CrawlerCharacterScreenData screenData = new CrawlerCharacterScreenData()
            {
                Unit = member,
                PrevState = currentData.Id,
            };

            InventoryData idata = member.Get<InventoryData>();

            idata.SetInvenEquip(partyData.Inventory, member.Equipment);

            _screenService.Open(ScreenId.CrawlerCharacter, screenData);
            await Task.CompletedTask;
            return crawlerStateData;


        }
    }
}
