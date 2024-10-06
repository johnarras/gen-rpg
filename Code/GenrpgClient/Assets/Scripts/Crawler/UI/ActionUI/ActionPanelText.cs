using Assets.Scripts.MVC;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler.ActionUI
{
    public class ActionPanelText : BaseViewController<TextAction,IView>
    {
        public IButton Button;
        public IText Text;
        private string _text = null;

        public override async Task Init(TextAction model, IView view, CancellationToken token)
        {
            await base.Init(model, view, token);
            Text = _view.Get<IText>("Text");
            Button = _view.Get<IButton>("Button");

            _text = model.Text;
            _uiService.SetText(Text, _text);

            if (Button != null && model.ClickAction != null)
            {
                _uiService.SetButton(Button, "APT", () => { model.ClickAction(); });
            }
        }
    }
}
