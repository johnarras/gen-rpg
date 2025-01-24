using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers
{
    public interface IStateHelper : ISetupDictionaryItem<ECrawlerStates>
    {
        Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token);
        bool IsTopLevelState();
        long TriggerBuildingId();
        long TriggerDetailEntityTypeId();
    }
}
