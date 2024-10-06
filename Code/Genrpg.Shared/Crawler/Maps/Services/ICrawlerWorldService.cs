using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Zones.Settings;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Services
{
    public interface ICrawlerWorldService : IInjectable
    {

        Task<CrawlerWorld> GenerateWorld(PartyData partyData);
        Task<CrawlerWorld> GetWorld(long worldId);
        CrawlerMap GetMap(long mapId);

        Task SaveWorld(CrawlerWorld world);

        Task<ZoneType> GetCurrentZone(PartyData partyData);
        Task<long> GetMapLevelAtPoint(CrawlerWorld world, long mapId, int x, int z);
        Task<long> GetMapLevelAtParty(CrawlerWorld world, PartyData partyData);

    }
}
