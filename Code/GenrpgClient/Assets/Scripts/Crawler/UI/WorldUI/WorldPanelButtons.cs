﻿using Assets.Scripts.MVC;
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
        public override async Task Init(WorldPanel model, IView view, CancellationToken token)
        {
            await base.Init(model, view, token);

            _mapButton = view.Get<IButton>("MapButton");

            _uiService.SetButton(_mapButton, GetType().Name, ClickMapScreen);
        }

        private void ClickMapScreen()
        {
            _screenService.Open(ScreenId.CrawlerMap);
        }      
    }
}