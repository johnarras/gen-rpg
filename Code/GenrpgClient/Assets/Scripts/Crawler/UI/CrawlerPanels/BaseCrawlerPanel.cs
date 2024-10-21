using Assets.Scripts.MVC;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.Tasks.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public abstract class BaseCrawlerPanel : BaseViewController<CrawlerScreen,IView>
    {

        protected ICrawlerService _crawlerService;
        protected ITaskService _taskService;
        public abstract Task OnNewStateData(CrawlerStateData stateData, CancellationToken token);

        public override async Task Init(CrawlerScreen model, IView view, CancellationToken token)
        {
            await base.Init(model, view, token);
        }
    }
}
