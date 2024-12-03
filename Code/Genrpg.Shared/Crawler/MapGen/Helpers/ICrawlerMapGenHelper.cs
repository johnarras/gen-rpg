using Genrpg.Shared.Crawler.MapGen.Entities;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.MapGen.Helpers
{
    public interface ICrawlerMapGenHelper : ISetupDictionaryItem<long>
    {
        Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData crawlerMapGenData);
    }
}
