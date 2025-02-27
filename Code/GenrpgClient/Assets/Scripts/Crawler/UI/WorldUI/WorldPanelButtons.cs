using Assets.Scripts.MVC;
using Assets.Scripts.UI.Crawler;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class WorldPanelButtons : BaseViewController<WorldPanel,IView>
    {

        private ICrawlerService _crawlerService;


        private IButton _mapButton;
        private IButton _safetyButton;
        private IButton _infoButton;
        private IButton _mainMenuButton;
        public override async Task Init(WorldPanel model, IView view, CancellationToken token)
        {
            await base.Init(model, view, token);

            _mapButton = view.Get<IButton>("MapButton");
            _safetyButton = view.Get<IButton>("SafetyButton");
            _infoButton = view.Get<IButton>("InfoButton");
            _mainMenuButton = view.Get<IButton>("MainMenuButton");
            _uiService.SetButton(_mapButton, GetType().Name, ClickMapScreen);
            _uiService.SetButton(_safetyButton, GetType().Name, ClickSafety);
            _uiService.SetButton(_infoButton, GetType().Name, ClickInfo);
            _uiService.SetButton(_mainMenuButton, GetType().Name, ClickMainMenu);
        }

        private void ClickMapScreen()
        {
            PartyData partyData = _crawlerService.GetParty();

            if (partyData.Buffs.Get(PartyBuffs.Mapping) == 0)
            {
                _dispatcher.Dispatch(new ShowFloatingText("You can only look at maps when mapping is active.", EFloatingTextArt.Error));
                return;
            }

            _screenService.Open(ScreenId.CrawlerMap);
        }

        private void ClickMainMenu()
        {
            _screenService.Open(ScreenId.CrawlerMainMenu);
        }      

        private void ClickSafety()
        {
            _crawlerService.ChangeState(ECrawlerStates.ReturnToSafety, _token);
        }

        private void ClickInfo()
        {
            _screenService.Open(ScreenId.CrawlerInfo);
        }
    }
}
