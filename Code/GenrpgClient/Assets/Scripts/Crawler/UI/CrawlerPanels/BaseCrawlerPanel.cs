using Assets.Scripts.Crawler.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public abstract class BaseCrawlerPanel : BaseBehaviour
    {

        protected ICrawlerService _crawlerService;

        protected CrawlerScreen _screen;

        protected CancellationToken _token;

        public abstract void OnNewStateData(CrawlerStateData stateData);

        public virtual async Awaitable Init(CrawlerScreen screen, CancellationToken token)
        {
            _screen = screen;
            _token = token;
            await Task.CompletedTask;
        }
    }
}
