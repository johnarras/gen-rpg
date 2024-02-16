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
    public class ActionPanelRow : BaseBehaviour, IPointerEnterHandler
    {

        private ICrawlerService _crawlerService;

        public GButton Button;
        public GText Text;

        protected CrawlerStateAction _action = null;
        protected CrawlerStateData _data = null;
        protected CancellationToken _token;
        public void Init(CrawlerStateData data, CrawlerStateAction action, CancellationToken token)
        {
            _action = action;
            _data = data;
            _token = token;

            if (_action != null)
            {
                string text = action.Text;

                if (_action.Key == KeyCode.Escape)
                {
                    text = "\n\nPress <color=yellow>Escape</color> to return to " + StrUtils.SplitOnCapitalLetters(_action.NextState.ToString());                
                }
                else if (text != null && text.Length > 0 && char.IsLetterOrDigit(text[0]))
                {
                    if (char.ToUpper(text[0]) == (char)(_action.Key) ||
                        char.ToLower(text[0]) == (char)(_action.Key))
                    {
                        char firstLetter = text[0];
                        text = "<color=yellow>[" + text[0] + "]</color>" + text.Substring(1);
                    }
                }

                _uiService.SetText(Text, text);

                _uiService.SetButton(Button, "ActionTextRow", ClickAction);

            }
        }

        private void ClickAction()
        {
            if (_action != null && _action.NextState != ECrawlerStates.None)
            {
                _crawlerService.ChangeState(_data, _action, _token);  
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_action != null && !string.IsNullOrEmpty(_action.SpriteName))
            {
                _gs.Dispatch<ShowWorldPanelImage>(new ShowWorldPanelImage()
                {
                    SpriteName = _action.SpriteName
                });
            }
        }
    }
}
