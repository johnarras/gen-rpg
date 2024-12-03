using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Vendors
{
    public class VendorHelper : BaseStateHelper
    {
        private IScreenService _screenService = null;
        public override ECrawlerStates GetKey() { return ECrawlerStates.Vendor; }
        public override long TriggerBuildingId() { return BuildingTypes.Equipment; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData crawlerStateData = CreateStateData();
            crawlerStateData.WorldSpriteName = CrawlerClientConstants.VendorImage;
            _screenService.Open(ScreenId.CrawlerVendor);

            await Task.CompletedTask;
            return crawlerStateData;
        }
    }
}
