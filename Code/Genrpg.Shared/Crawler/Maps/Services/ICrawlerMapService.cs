﻿using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;
using System.Threading;

namespace Genrpg.Shared.Crawler.Maps.Services
{
    public interface ICrawlerMapService : IInitializable
    {
        Task EnterMap(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token);
        Task UpdateMovement(CancellationToken token);
        void CleanMap();
        void ClearMovement();
        bool UpdatingMovement();
        void SetUpdatingMovement(bool updatingMovement);
        string GetBuildingArtPrefix();
        void MarkCurrentCellVisited();
        void MarkCellVisited(long mapId, int x, int z);
        bool PartyHasVisited(long mapId, int x, int z, bool thisRunOnly = false);
        int GetBlockingBits(int sx, int sz, int ex, int ez, bool allowBuildingEntry);
        void MovePartyTo(PartyData partyData, int x, int z, int rot, CancellationToken token, bool rotationOnly = false);
        FullWallTileImage GetMinimapWallFilename(CrawlerMap map, int x, int z);
        Task AddKeyInput(char keyChar, CancellationToken token);

    }
}