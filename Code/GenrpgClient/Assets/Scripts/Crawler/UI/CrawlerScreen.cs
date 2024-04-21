using Assets.Scripts.Crawler.Services;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler
{
    public class CrawlerScreen : BaseScreen
    {
        private ICrawlerService _crawlerService;

        public WorldPanel WorldPanel;
        public ActionPanel ActionPanel;
        public StatusPanel StatusPanel;

        protected override async UniTask OnStartOpen(object data, CancellationToken token)
        {
            await _crawlerService.LoadSaveGame();
            _dispatcher.AddEvent<CrawlerStateData>(this, OnNewStateData);
            await WorldPanel.Init(this,token);
            await ActionPanel.Init(this, token);
            await StatusPanel.Init(this, token);
            await _crawlerService.Init(token);

        }

        private CrawlerStateData OnNewStateData(UnityGameState gs, CrawlerStateData data)
        {
            WorldPanel.OnNewStateData(data);
            ActionPanel.OnNewStateData(data);
            StatusPanel.OnNewStateData(data);
            return null;
        }
    }
}
