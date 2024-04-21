using Cysharp.Threading.Tasks;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapObjects.Entities;
using GEntity = UnityEngine.GameObject;
using System.Threading;
using Genrpg.Shared.MapObjects.Messages;
using Assets.Scripts.MapTerrain;
using UnityEngine;
using Genrpg.Shared.GameSettings;

/// <summary>
/// Base class for object loaders
/// 
/// *** NOTE IF YOU MAKE A NEW ONE OF THESE YOU MUST REGISTER IT IN Map
/// </summary>
public abstract class BaseMapObjectLoader : IMapObjectLoader
{
    public abstract long GetKey();

    public abstract UniTask Load(UnityGameState gs, OnSpawn message, MapObject loadedObject, CancellationToken token);

    protected abstract string GetLayerName();

    protected IMapTerrainManager _terrainManager;
    protected IAssetService _assetService;
    protected IClientMapObjectManager _objectManager;
    protected IGameData _gameData;

    public void FinalPlaceObject(UnityGameState gs, GEntity go, SpawnLoadData data, string layerName)
    {
        if (go == null)
        {
            return;
        }

        TerrainPatchData patchData = _terrainManager.GetPatchFromMapPos(gs, data.Spawn.X, data.Spawn.Z);

        if (patchData == null)
        {
            return;
        }
        Terrain terrain = patchData.terrain as Terrain;
        if (terrain != null)
        {
            GEntityUtils.AddToParent(go, terrain.entity());
        }
        else
        {
            GEntityUtils.Destroy(go);
            return;
        }

        GEntityUtils.SetLayer(go, LayerUtils.NameToLayer(layerName));

        long placementSeed = (long)(data.Spawn.X * 131 + data.Spawn.Z * 517);

        float nx = MathUtils.SeedFloatRange(placementSeed * 13, 143, -0.5f, 0.5f, 101) + data.Spawn.X;
        float nz = MathUtils.SeedFloatRange(placementSeed * 17, 149, -0.5f, 0.5f, 101) + data.Spawn.Z;

        if (data.FixedPosition)
        {
            nx = data.Spawn.X;
            nz = data.Spawn.Z;
        }

        float height = _terrainManager.SampleHeight(gs, nx, nz);
       
        go.transform().position = GVector3.Create(nx, height, nz);
        go.transform().eulerAngles = GVector3.Create(0, data.Spawn.Rot, 0);

        if (data.Obj is Character ch)
        {
            go.transform().position += GVector3.Create(0, 2, 0);
        }

        _objectManager.AddObject(data.Obj, go);
    }
}