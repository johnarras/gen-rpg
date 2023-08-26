using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Constants;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Messages;

public class GroundObjectLoader : BaseMapObjectLoader
{
    public GroundObjectLoader(UnityGameState gs) : base(gs)
    {

    }

    public override long GetKey() { return EntityType.GroundObject; }
    protected override string GetLayerName() { return LayerNames.ObjectLayer; }

    public override async UniTask Load(UnityGameState gs, OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        GroundObjType groundObjType = gs.data.GetGameData<ProcGenSettings>().GetGroundObjType(spawn.EntityId);
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

        _assetService.LoadAsset(gs, AssetCategory.Props, groundObjType.Art, OnDownloadGroundObject, loadData, null, token);

        await Task.CompletedTask;
        return;
    }

    private void OnDownloadGroundObject(UnityGameState gs, string url, object obj, object data, CancellationToken token)
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

        MapGroundObject worldGroundObject = GameObjectUtils.GetOrAddComponent<MapGroundObject>(gs,go);

        GroundObjType gtype = gs.data.GetGameData<ProcGenSettings>().GetGroundObjType(loadData.Spawn.EntityId);

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



