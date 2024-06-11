
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public interface ICrawlerMapTypeHelper : ISetupDictionaryItem<ECrawlerMapTypes>
    {

        Awaitable<CrawlerMapRoot> Enter(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token);

        int GetBlockingBits(CrawlerMapRoot mapRoot, int startx, int startz, int endx, int endz);

        Awaitable DrawCell(CrawlerMapRoot mapRoot, UnityMapCell cell, int xpos, int zpos, CancellationToken token);
    }
}
