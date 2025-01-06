using Assets.Scripts.MVC;
using Assets.Scripts.UI.Crawler;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class WorldPanelButtons : BaseViewController<WorldPanel,IView>
    {
        private IButton _mapButton;
        private IButton _resetButton;
        private IButton _infoButton;
        public override async Task Init(WorldPanel model, IView view, CancellationToken token)
        {
            await base.Init(model, view, token);

            _mapButton = view.Get<IButton>("MapButton");
            _resetButton = view.Get<IButton>("ResetButton");
            _infoButton = view.Get<IButton>("InfoButton");

            _uiService.SetButton(_mapButton, GetType().Name, ClickMapScreen);
            _uiService.SetButton(_resetButton, GetType().Name, ClickResetGame);
            _uiService.SetButton(_infoButton, GetType().Name, ClickInfo);
        }

        private void ClickMapScreen()
        {
            _screenService.Open(ScreenId.CrawlerMap);
        }      

        private void ClickResetGame()
        {
            _initClient.FullResetGame();
        }

        private void ClickInfo()
        {
            _screenService.Open(ScreenId.CrawlerInfo);
        }
    }
}
