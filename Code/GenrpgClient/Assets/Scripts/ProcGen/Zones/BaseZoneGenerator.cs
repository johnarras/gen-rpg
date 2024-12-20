﻿using Assets.Scripts.Awaitables;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.ProcGen.Services;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BaseZoneGenerator : IZoneGenerator, IInitializable
{
    protected IAssetService _assetService;
    protected IFileDownloadService _fileDownloadService;
    protected IClientMapObjectManager _objectManager;
    protected IMapTerrainManager _terrainManager;
    protected INoiseService _noiseService;
    protected IZoneGenService _zoneGenService;
    protected CancellationToken _token;
    protected ILogService _logService;
    protected IRepositoryService _repoService;
    protected IDispatcher _dispatcher;
    protected IGameData _gameData;
    protected IMapProvider _mapProvider;
    protected IClientGameState _gs;
    protected IClientRandom _rand;
    protected IMapGenData _md;
    protected IPathfindingService _pathfindingService;
    protected IClientEntityService _clientEntityService;
    protected IAwaitableService _awaitableService;

    public virtual async Awaitable Generate(CancellationToken token)
    {
        _token = token;
        await Task.CompletedTask;

    }

    public virtual async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }
}