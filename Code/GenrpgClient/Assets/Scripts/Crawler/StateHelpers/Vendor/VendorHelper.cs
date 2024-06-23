using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Exploring
{
    public class VendorHelper : BaseStateHelper
    {
        private IScreenService _screenService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.Vendor; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {

            CrawlerStateData crawlerStateData = CreateStateData();
            crawlerStateData.WorldSpriteName = CrawlerClientConstants.VendorImage;
            _screenService.Open(ScreenId.CrawlerVendor);

            await Task.CompletedTask;
            return crawlerStateData;
        }
    }
}
