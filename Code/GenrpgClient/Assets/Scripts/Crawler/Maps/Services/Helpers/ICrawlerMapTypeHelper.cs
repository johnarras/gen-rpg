
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public interface ICrawlerMapTypeHelper : ISetupDictionaryItem<long>
    {

        Awaitable<CrawlerMapRoot> EnterMap(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token);

        int GetBlockingBits(CrawlerMapRoot mapRoot, int startx, int startz, int endx, int endz, bool allowBuildingEntry);

        Awaitable DrawCell(CrawlerWorld world, PartyData party, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token);
    }
}
