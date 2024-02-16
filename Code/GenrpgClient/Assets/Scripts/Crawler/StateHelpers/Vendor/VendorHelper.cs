using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.UI.Screens.Characters;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Inventory.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace Assets.Scripts.Crawler.StateHelpers.Exploring
{
    public class VendorHelper : BaseStateHelper
    {
        private IScreenService _screenService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.Vendor; }

        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {

            CrawlerStateData crawlerStateData = CreateStateData();
            _screenService.Open(gs, ScreenId.CrawlerVendor);
            await UniTask.CompletedTask;
            return crawlerStateData;

            
        }
    }
}
