
using Assets.Scripts.MVC;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Exploring;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.UI.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler
{
    public class CrawlerScreen : BaseScreen
    {
        private ICrawlerService _crawlerService;
        protected ITaskService _taskService;

        private WorldPanel _worldPanel;
        private ActionPanel _actionPanel;
        private StatusPanel _statusPanel;


        private object _worldParent;
        private object _statusParent;
        private object _actionParent;

        public BaseView View;

        public string WorldPanelName = "WorldPanel";
        public string ActionPanelName = "ActionPanel";
        public string StatusPanelName = "StatusPanel";

        public string ActionPanelElementSuffix = "";

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            PartyData partyData = await _crawlerService.LoadParty();
            AddListener<CrawlerStateData>(OnNewStateData);

            _worldParent = View.Get<object>("World");
            _statusParent = View.Get<object>("Status");
            _actionParent = View.Get<object>("Action");

            _worldPanel = await _assetService.CreateAsync<WorldPanel, CrawlerScreen>(this,
                AssetCategoryNames.UI, WorldPanelName, _worldParent, _token, Subdirectory);

            _actionPanel = await _assetService.CreateAsync<ActionPanel, CrawlerScreen>(this,
                AssetCategoryNames.UI, ActionPanelName, _actionParent, _token, Subdirectory);
            _statusPanel = await _assetService.CreateAsync<StatusPanel, CrawlerScreen>(this,
                AssetCategoryNames.UI, StatusPanelName, _statusParent, _token, Subdirectory);

            partyData.WorldPanel = _worldPanel;
            partyData.StatusPanel = _statusPanel;
            partyData.ActionPanel = _actionPanel;

            await _crawlerService.Init(partyData, token);

            _screenService.CloseAll(new List<ScreenId>() { _crawlerService.GetCrawlerScreenId()});

            _dispatcher.AddListener<CrawlerCharacterScreenData>(OnCrawlerCharacterData, GetToken());
        }

        private void OnNewStateData(CrawlerStateData data)
        {
            _taskService.ForgetTask(OnNewStatDataAsync(data, _token));
        }

        private async Task OnNewStatDataAsync(CrawlerStateData data, CancellationToken token)
        {
            await _worldPanel.OnNewStateData(data, token);
            await _statusPanel.OnNewStateData(data, token);
            await _actionPanel.OnNewStateData(data, token);
        }

        private void OnCrawlerCharacterData(CrawlerCharacterScreenData data)
        {
            _screenService.Open(ScreenId.CrawlerCharacter, data);
        }
    }
}
