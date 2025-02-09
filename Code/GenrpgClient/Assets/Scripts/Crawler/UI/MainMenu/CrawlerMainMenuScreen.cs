using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.LoadSave.Services;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.MainMenu
{
    public class CrawlerMainMenuScreen : MainMenuScreen
    {
        public GButton ContinueGameButton;
        public GButton LoadGameButton;
        public GButton NewCrawlerGameButton;
        public GButton NewRogueliteGameButton;
        public GButton CloseButton;

        protected IInputService _inputService;
        private ILoadSaveService _loadSaveService;
        private ICrawlerService _crawlerService;
        private IClientAppService _clientAppService;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await base.OnStartOpen(data, token);



            _uiService.SetButton(ContinueGameButton, GetName(), ClickContinue);
            _uiService.SetButton(QuitGameButton, GetName(), ClickQuit);
            _uiService.SetButton(NewCrawlerGameButton, GetName(), ClickNewCrawler);
            _uiService.SetButton(NewRogueliteGameButton, GetName(), ClickNewRoguelite);
            _uiService.SetButton(LoadGameButton, GetName(), ClickLoadGame);


            if (!_loadSaveService.HaveCurrentGame<PartyData>())
            {
                _uiService.SetInteractable(ContinueGameButton, false);
            }

            if (_screenService.GetScreen(ScreenId.Crawler) == null)
            {
                _clientEntityService.SetActive(CloseButton, false);
            }
        }

        protected override void ScreenUpdate()
        {
            base.ScreenUpdate();

            if (_inputService.GetKeyDown(CharCodes.Escape))
            {
                _screenService.Close(ScreenId.CrawlerMainMenu);
            }
        }

        private void ClickNewCrawler()
        {
            _crawlerService.NewGame(ECrawlerGameModes.Crawler);
        }


        private void ClickNewRoguelite()
        {
            _crawlerService.NewGame(ECrawlerGameModes.Roguelite);
        }

        private void ClickContinue()
        {
            _crawlerService.ContinueGame();
        }

        private void ClickLoadGame()
        {
            _screenService.CloseAll();
            _screenService.Open(ScreenId.LoadSave);
        }

        private void ClickQuit()
        {
            _clientAppService.Quit();
        }
    }
}
