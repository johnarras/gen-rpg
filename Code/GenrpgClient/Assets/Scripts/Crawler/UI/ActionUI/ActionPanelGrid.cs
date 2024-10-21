using Assets.Scripts.MVC;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using Genrpg.Shared.UI.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.ActionUI
{
    public class ActionPanelGrid : BaseViewController<bool,IView>
    {

        public IGridLayoutGroup Group;

        public override async Task Init(bool useSmallerButtons, IView view, CancellationToken token)
        {
            await base.Init(useSmallerButtons, view, token);
            Group = _view.Get<IGridLayoutGroup>("LayoutGroup");
            if (useSmallerButtons)
            {
                _uiService.ResizeGridLayout(Group, 2, 2);
            }
        }

        public object GetContentRoot()
        {
            return _clientEntityService.GetEntity(Group);
        }
    }
}
