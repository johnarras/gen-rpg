using Assets.Scripts.ProcGen.RandomNumbers;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapServer.Services;
using System.Threading;
using System.Threading.Tasks;

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
    protected IUnityGameState _gs;
    protected IClientRandom _rand;
    protected IMapGenData _md;

    public virtual async UniTask Generate(CancellationToken token)
    {
        _token = token;
        await UniTask.CompletedTask;
    }

    public virtual async Task Initialize(IGameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }
}