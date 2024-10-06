using Assets.Scripts.ClientEvents;
using Assets.Scripts.MVC;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler.ActionUI
{
    public class ActionPanelRow : BaseViewController<CrawlerStateWithAction,IView>
    {

        private ICrawlerService _crawlerService;
        private ITextService _textService;
        public IView View;

        IText Text;
        IButton Button;
        protected CrawlerStateAction _action = null;
        protected CrawlerStateData _state = null;

        public override async Task Init(CrawlerStateWithAction model, IView view, CancellationToken token)
        {
            await base.Init(model, view, token);
            _action = model.Action;
            _state = model.State;

            Text = _view.Get<IText>("Text");
            Button = _view.Get<IButton>("Button");

            _uiService.AddPointerHandlers(view, OnPointerEnter, OnPointerExit);

            if (_action != null)
            {
                string text = _action.Text;

                if (_action.Key == CharCodes.Escape)
                {
                    text = $"\n\nPress {_textService.HighlightText("Escape")} to return to " + StrUtils.SplitOnCapitalLetters(_action.NextState.ToString());                
                }
                else if (text != null && text.Length > 0 && char.IsLetterOrDigit(text[0]))
                {
                    if (char.ToUpper(text[0]) == (char)(_action.Key) ||
                        char.ToLower(text[0]) == (char)(_action.Key))
                    {
                        char firstLetter = text[0];
                        text = $"{_textService.HighlightText(text[0])} {text.Substring(1)}";
                    }
                }

                if (_state.UseSmallerButtons)
                {
                    _uiService.SetAutoSizing(Text, true);
                }


                _uiService.SetText(Text, text);

                _uiService.SetButton(Button, "ActionTextRow", ClickAction);

            }
        }

        private void ClickAction()
        {
            if (_action != null && _action.NextState != ECrawlerStates.None)
            {
                _crawlerService.ChangeState(_state, _action, _token);  
            }
        }

        public void OnPointerExit()
        {
            _uiService.SetAlpha(Text, 1.0f);
            if (_action != null && _action.OnPointerExit != null)
            {
                _action?.OnPointerExit();
            }
            else
            {
                _dispatcher.Dispatch(new HideCrawlerTooltipEvent());
            }
        }

        public void OnPointerEnter()
        {

            _uiService.SetAlpha(Text, 0.5f);
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
