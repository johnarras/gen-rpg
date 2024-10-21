
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapObjects.Entities;
using UnityEngine;
using System.Threading;
using Genrpg.Shared.MapObjects.Messages;
using Assets.Scripts.MapTerrain;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.Assets;
using Genrpg.Shared.Client.Assets.Services;
using Assets.Scripts.Awaitables;

/// <summary>
/// Base class for object loaders
/// 
/// *** NOTE IF YOU MAKE A NEW ONE OF THESE YOU MUST REGISTER IT IN Map
/// </summary>
public abstract class BaseMapObjectLoader : IMapObjectLoader
{
    public abstract long GetKey();

    public abstract Awaitable Load(OnSpawn message, MapObject loadedObject, CancellationToken token);

    protected abstract string GetLayerName();

    protected IMapTerrainManager _terrainManager;
    protected IAssetService _assetService;
    protected IClientMapObjectManager _objectManager;
    protected IGameData _gameData;
    protected IMapProvider _mapProvider;
    protected IClientGameState _gs;
    protected IClientEntityService _clientEntityService;
    protected IAwaitableService _awaitableService;

    public void FinalPlaceObject(GameObject go, SpawnLoadData data, string layerName)
    {
        if (go == null)
        {
            return;
        }

        TerrainPatchData patchData = _terrainManager.GetPatchFromMapPos(data.Spawn.X, data.Spawn.Z);

        if (patchData == null)
        {
            return;
        }
        Terrain terrain = patchData.terrain as Terrain;
        if (terrain != null)
        {
            _clientEntityService.AddToParent(go, terrain.gameObject);
        }
        else
        {
            _clientEntityService.Destroy(go);
            return;
        }

        _clientEntityService.SetLayer(go, LayerUtils.NameToLayer(layerName));

        long placementSeed = (long)(data.Spawn.X * 131 + data.Spawn.Z * 517);

        float nx = MathUtils.SeedFloatRange(placementSeed * 13, 143, -0.5f, 0.5f, 101) + data.Spawn.X;
        float nz = MathUtils.SeedFloatRange(placementSeed * 17, 149, -0.5f, 0.5f, 101) + data.Spawn.Z;

        if (data.FixedPosition)
        {
            nx = data.Spawn.X;
            nz = data.Spawn.Z;
        }

        float height = _terrainManager.SampleHeight(nx, nz);
       
        go.transform.position = new Vector3(nx, height, nz);
        go.transform.eulerAngles = new Vector3(0, data.Spawn.Rot, 0);

        if (data.Obj is Character ch)
        {
            go.transform.position += new Vector3(0, 2, 0);
        }

        _objectManager.AddObject(data.Obj, go);
    }
}