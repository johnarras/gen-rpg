using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Utils;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Crawler.ActionUI
{
    public class ActionPanelText : BaseBehaviour
    {

        public GText Text;

        private string _text = null;
        public void Init(string text, CancellationToken token)
        {
            _text = text;
            _uiService.SetText(Text, text);
        }
    }
}
