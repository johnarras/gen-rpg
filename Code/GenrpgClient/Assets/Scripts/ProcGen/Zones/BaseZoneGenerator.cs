using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
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

    public virtual async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        _token = token;
        gs.loc.Resolve(this);
        await UniTask.CompletedTask;
    }

    public virtual async Task Initialize(GameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }
}