using Cysharp.Threading.Tasks;
using System.Threading;

public class BaseZoneGenerator : IZoneGenerator
{
    protected IAssetService _assetService;
    protected IClientMapObjectManager _objectManager;
    protected IMapTerrainManager _terrainManager;
    protected INoiseService _noiseService;
    protected IZoneGenService _zoneGenService;
    protected CancellationToken _token;

    public virtual async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        _token = token;
        gs.loc.Resolve(this);
        await UniTask.CompletedTask;
    }
}