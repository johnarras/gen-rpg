using Assets.Scripts.MVC;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler.WorldUI
{
    public class WorldPanelText : BaseViewController<string,IView>
    {
        private IText _text;

        public override async Task Init(string newText, IView view, CancellationToken token)
        {
            await base.Init(newText, view, token);

            _text = view.Get<IText>("Text");

            _uiService.SetText(_text, newText);
        }
    }
}
