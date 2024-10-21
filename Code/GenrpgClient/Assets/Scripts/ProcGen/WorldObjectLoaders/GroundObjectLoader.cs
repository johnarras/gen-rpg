
using UnityEngine;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Constants;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.GroundObjects.Settings;
using System.Threading.Tasks;
using Genrpg.Shared.Client.Assets.Constants;

public class GroundObjectLoader : BaseMapObjectLoader
{
    public override long GetKey() { return EntityTypes.GroundObject; }
    protected override string GetLayerName() { return LayerNames.ObjectLayer; }

    public override async Awaitable Load(OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        GroundObjType groundObjType = _gameData.Get<GroundObjTypeSettings>(_gs.ch).Get(spawn.EntityId);
        if (groundObjType == null)
        {
            return;
        }
        float wx = spawn.X;
        float wz = spawn.Z;

        SpawnLoadData loadData = new SpawnLoadData()
        {
            Obj = obj,
            Spawn = spawn,
            Token = token,
        };

        _assetService.LoadAsset(AssetCategoryNames.Props, groundObjType.Art, OnDownloadGroundObject, loadData, null, token);


        await Task.CompletedTask;
        return;
    }

    private void OnDownloadGroundObject(object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        SpawnLoadData loadData = data as SpawnLoadData;
        if (loadData ==null)
        {
            return;
        }

        MapGroundObject worldGroundObject = _clientEntityService.GetOrAddComponent<MapGroundObject>(go);

        GroundObjType gtype = _gameData.Get<GroundObjTypeSettings>(_gs.ch).Get(loadData.Spawn.EntityId);

        worldGroundObject.GroundObjectId = gtype.IdKey;
        worldGroundObject.CrafterTypeId = gtype.CrafterTypeId;
        worldGroundObject.Init(loadData.Obj,go, token);
        worldGroundObject.GroundObj = gtype;        
        worldGroundObject.X = (int)loadData.Spawn.X;
        worldGroundObject.Z = (int)loadData.Spawn.Z;
        if (loadData.Spawn.ZoneId > 0)
        {
            Zone zone = _mapProvider.GetMap().Get<Zone>(loadData.Spawn.ZoneId);
            if (zone != null)
            {
                worldGroundObject.Level = zone.Level;
            }
        }

        if (gtype.CrafterTypeId > 0 && gtype.SpawnTableId > 0)
        {
            worldGroundObject.ShowGlow(0);
        }
        FinalPlaceObject(go, loadData, LayerNames.ObjectLayer);
    }
}



