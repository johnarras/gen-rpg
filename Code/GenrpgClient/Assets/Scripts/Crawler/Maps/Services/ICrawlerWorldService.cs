using Assets.Scripts.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Zones.Settings;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services
{
    public interface ICrawlerWorldService : IInjectable
    {

        Awaitable<CrawlerWorld> GenerateWorld(PartyData partyData);
        Awaitable<CrawlerWorld> GetWorld(long worldId);
        CrawlerMap GetMap(long mapId);

        Awaitable SaveWorld(CrawlerWorld world);

        Awaitable<ZoneType> GetCurrentZone(PartyData partyData);
        Awaitable<long> GetMapLevelAtPoint(CrawlerWorld world, long mapId, int x, int z);
        Awaitable<long> GetMapLevelAtParty(CrawlerWorld world, PartyData partyData);

    }
}
