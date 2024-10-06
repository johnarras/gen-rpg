
using Assets.Scripts.MVC;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler
{
    public class CrawlerScreen : BaseScreen
    {
        private ICrawlerService _crawlerService;

        private WorldPanel _worldPanel;
        private ActionPanel _actionPanel;
        private StatusPanel _statusPanel;

        private object _leftParent;
        private object _rightParent;

        public BaseView View;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            PartyData partyData = await _crawlerService.LoadParty();
            AddListener<CrawlerStateData>(OnNewStateData);

            _leftParent = View.Get<object>("Left");
            _rightParent = View.Get<object>("Right");

            _worldPanel = await _assetService.CreateAsync<WorldPanel, CrawlerScreen>(this,
                AssetCategoryNames.UI, "WorldPanel", _leftParent, _token, Subdirectory);
            _actionPanel = await _assetService.CreateAsync<ActionPanel, CrawlerScreen>(this,
                AssetCategoryNames.UI, "ActionPanel", _rightParent, _token, Subdirectory);
            _statusPanel = await _assetService.CreateAsync<StatusPanel, CrawlerScreen>(this,
                AssetCategoryNames.UI, "StatusPanel", _rightParent, _token, Subdirectory);

            partyData.WorldPanel = _worldPanel;
            partyData.StatusPanel = _statusPanel;
            partyData.ActionPanel = _actionPanel;

            await _crawlerService.Init(partyData, token);
        }

        private void OnNewStateData(CrawlerStateData data)
        {
            TaskUtils.ForgetTask(OnNewStatDataAsync(data, _token));
        }

        private async Task OnNewStatDataAsync(CrawlerStateData data, CancellationToken token)
        {
            await _worldPanel.OnNewStateData(data, token);
            await _statusPanel.OnNewStateData(data, token);
            await _actionPanel.OnNewStateData(data, token);
        }
    }
}
