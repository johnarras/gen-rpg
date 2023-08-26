using Cysharp.Threading.Tasks;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapObjects.Entities;
using UnityEngine;
using System.Threading;
using Genrpg.Shared.MapObjects.Messages;
using Assets.Scripts.MapTerrain;

public abstract class BaseMapObjectLoader : IMapObjectLoader
{
    public BaseMapObjectLoader(UnityGameState gs)
    {
        gs.loc.Resolve(this);
    }

    public abstract long GetKey();

    public abstract UniTask Load(UnityGameState gs, OnSpawn message, MapObject loadedObject, CancellationToken token);

    protected abstract string GetLayerName();

    protected IMapTerrainManager _mapTerrainManager;
    protected IAssetService _assetService;
    protected IClientMapObjectManager _objectManager;

    public void FinalPlaceObject(UnityGameState gs, GameObject go, SpawnLoadData data, string layerName)
    {
        if (go == null)
        {
            return;
        }

        TerrainPatchData patchData = _mapTerrainManager.GetPatchFromMapPos(gs, data.Spawn.X, data.Spawn.Z);

        if (patchData == null)
        {
            return;
        }
        Terrain terrain = patchData.terrain as Terrain;
        if (terrain != null)
        {
            GameObjectUtils.AddToParent(go, terrain.gameObject);
        }
        else
        {
            GameObject.Destroy(go);
            return;
        }

        GameObjectUtils.SetLayer(go, LayerMask.NameToLayer(layerName));

        long placementSeed = (long)(data.Spawn.X * 131 + data.Spawn.Z * 517);

        float nx = MathUtils.SeedFloatRange(placementSeed * 13, 143, -0.5f, 0.5f, 101) + data.Spawn.X;
        float nz = MathUtils.SeedFloatRange(placementSeed * 17, 149, -0.5f, 0.5f, 101) + data.Spawn.Z;

        float height = gs.md.SampleHeight(gs, nx, MapConstants.MapHeight, nz);
       
        go.transform.position = new Vector3(nx, height, nz);
        go.transform.eulerAngles = new Vector3(0, data.Spawn.Rot, 0);

        if (data.Obj is Character ch)
        {
            go.transform.position += new Vector3(0, 2, 0);
        }

        _objectManager.AddObject(data.Obj, go);
    }
}