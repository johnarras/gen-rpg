using System.Threading;
using System.Threading.Tasks;

using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class PopStateHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.PopState; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.DoNotTransitionToThisState = true;

            _crawlerService.PopState();

            await Task.CompletedTask;
            return stateData;
        }
    }
}
