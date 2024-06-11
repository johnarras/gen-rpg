
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Constants;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Assets.Scripts.GroundObjects;
using UnityEngine;

public class MapModObjectLoader : BaseMapObjectLoader
{
    public override long GetKey() { return EntityTypes.MapMod; }
    protected override string GetLayerName() { return LayerNames.ObjectLayer; }

    public override async Awaitable Load(OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        float wx = spawn.X;
        float wz = spawn.Z;

        SpawnLoadData loadData = new SpawnLoadData()
        {
            Obj = obj,
            Spawn = spawn,
            Token = token,
        };

        _assetService.LoadAsset(AssetCategoryNames.Props, "MapMod", OnDownloadMapModObject, loadData, null, token);

        
        return;
    }

    private void OnDownloadMapModObject(object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        SpawnLoadData loadData = data as SpawnLoadData;
        if (loadData ==null)
        {
            return;
        }

        MapModObject mapModObject = go.GetComponent<MapModObject>();

        mapModObject.Init(loadData.Spawn);
        FinalPlaceObject(go, loadData, LayerNames.ObjectLayer);
        go.transform().position += GVector3.Create(0, 1, 0);
    }
}



