using Cysharp.Threading.Tasks;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Constants;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.GroundObjects.Settings;

public class GroundObjectLoader : BaseMapObjectLoader
{
    public override long GetKey() { return EntityTypes.GroundObject; }
    protected override string GetLayerName() { return LayerNames.ObjectLayer; }

    public override async UniTask Load(UnityGameState gs, OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        GroundObjType groundObjType = gs.data.Get<GroundObjTypeSettings>(gs.ch).Get(spawn.EntityId);
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

        _assetService.LoadAsset(gs, AssetCategoryNames.Props, groundObjType.Art, OnDownloadGroundObject, loadData, null, token);

        await UniTask.CompletedTask;
        return;
    }

    private void OnDownloadGroundObject(UnityGameState gs, object obj, object data, CancellationToken token)
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

        MapGroundObject worldGroundObject = GEntityUtils.GetOrAddComponent<MapGroundObject>(gs,go);

        GroundObjType gtype = gs.data.Get<GroundObjTypeSettings>(gs.ch).Get(loadData.Spawn.EntityId);

        worldGroundObject.GroundObjectId = gtype.IdKey;
        worldGroundObject.CrafterTypeId = gtype.CrafterTypeId;
        worldGroundObject.Init(loadData.Obj,go, token);
        worldGroundObject.GroundObj = gtype;        
        worldGroundObject.X = (int)loadData.Spawn.X;
        worldGroundObject.Z = (int)loadData.Spawn.Z;
        if (loadData.Spawn.ZoneId > 0)
        {
            Zone zone = gs.map.Get<Zone>(loadData.Spawn.ZoneId);
            if (zone != null)
            {
                worldGroundObject.Level = zone.Level;
            }
        }

        if (gtype.CrafterTypeId > 0 && gtype.SpawnTableId > 0)
        {
            worldGroundObject.ShowGlow(0);
        }
        FinalPlaceObject(gs, go, loadData, LayerNames.ObjectLayer);
    }
}



