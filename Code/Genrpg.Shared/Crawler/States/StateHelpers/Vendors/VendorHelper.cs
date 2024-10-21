using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Vendor
{
    public class VendorHelper : BaseStateHelper
    {
        private IScreenService _screenService;
        private ICrawlerMapService _crawlerMapService;
        public override ECrawlerStates GetKey() { return ECrawlerStates.Vendor; }

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
