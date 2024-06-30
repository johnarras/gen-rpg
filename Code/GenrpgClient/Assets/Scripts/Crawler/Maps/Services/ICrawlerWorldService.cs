using Assets.Scripts.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Zones.Settings;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services
{
    public interface ICrawlerWorldService : IInjectable
    {

        Awaitable<CrawlerWorld> GetWorld(long worldId);
        void SetWorld(CrawlerWorld world);

        Awaitable SaveWorld(CrawlerWorld world);

        CrawlerWorld Generate(long worldId);


        Awaitable<ZoneType> GetCurrentZone(PartyData partyData);
        Awaitable<long> GetMapLevelAtParty(PartyData partyData);

    }
}
