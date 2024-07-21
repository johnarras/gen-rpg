using Assets.Scripts.ClientEvents;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.UI.Utils;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Utils;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Crawler.ActionUI
{
    public class ActionPanelRow : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler
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
                    text = $"\n\nPress {CrawlerUIUtils.HighlightText("Escape")} to return to " + StrUtils.SplitOnCapitalLetters(_action.NextState.ToString());                
                }
                else if (text != null && text.Length > 0 && char.IsLetterOrDigit(text[0]))
                {
                    if (char.ToUpper(text[0]) == (char)(_action.Key) ||
                        char.ToLower(text[0]) == (char)(_action.Key))
                    {
                        char firstLetter = text[0];
                        text = $"{CrawlerUIUtils.HighlightText(text[0])} {text.Substring(1)}";
                    }
                }

                _uIInitializable.SetText(Text, text);

                _uIInitializable.SetButton(Button, "ActionTextRow", ClickAction);

            }
        }

        private void ClickAction()
        {
            if (_action != null && _action.NextState != ECrawlerStates.None)
            {
                _crawlerService.ChangeState(_data, _action, _token);  
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Text.alpha = 1.0f;
            if (_action != null && _action.OnPointerExit != null)
            {
                _action?.OnPointerExit();
            }
            else
            {
                _dispatcher.Dispatch(new HideCrawlerTooltipEvent());
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

            Text.alpha = 0.5f;
            if (_action != null)
            { 
                if (!string.IsNullOrEmpty(_action.SpriteName))
                {
                    _dispatcher.Dispatch<ShowWorldPanelImage>(new ShowWorldPanelImage()
                    {
                        SpriteName = _action.SpriteName
                    });
                }
                if (_action.OnPointerEnter != null)
                {
                    _action.OnPointerEnter();
                }
            }
        }
    }
}
