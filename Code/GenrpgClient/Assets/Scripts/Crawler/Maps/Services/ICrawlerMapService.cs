using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Services.CrawlerMaps
{
    public interface ICrawlerMapService : IInitializable
    {
        Awaitable EnterMap(PartyData partyData,  EnterCrawlerMapData mapData, CancellationToken token);
        Awaitable UpdateMovement(CancellationToken token);
        void CleanMap();
        void ClearMovement();
        bool UpdatingMovement();
        string GetBuildingArtPrefix();
        void MarkCurrentCellVisited();
        void MarkCellVisited(long mapId, int x, int z);
        bool PartyHasVisited(long mapId, int x, int z, bool thisRunOnly = false);
        int GetBlockingBits(int sx, int sz, int ex, int ez, bool allowBuildingEntry);
        void MovePartyTo(PartyData partyData, int x, int z, int rot, CancellationToken token, bool rotationOnly = false);
        FullWallTileImage GetMinimapWallFilename(CrawlerMap map, int x, int z);
    }
}
