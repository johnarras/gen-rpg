
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public interface ICrawlerMapGenHelper : ISetupDictionaryItem<long>
    {

        Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData crawlerMapGenData);

    }
}
