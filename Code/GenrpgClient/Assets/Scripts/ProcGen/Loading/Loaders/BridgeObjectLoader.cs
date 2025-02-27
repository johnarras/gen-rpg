﻿using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.ProcGen.Settings.Bridges;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using System.Threading;
using UnityEngine;

public class BridgeObjectLoader : BaseObjectLoader
{
    public override bool LoadObject(PatchLoadData loadData, uint objectId,
       int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {

        uint upperNumber = objectId >> 16;
        objectId = (objectId % (1 << 16)) % MapConstants.MapObjectOffsetMult;


        float btypeId = objectId;
        int angle = (int)(MapConstants.BridgeAngleDiv * (upperNumber & ((1 << MapConstants.BridgeHeightBitShift) - 1)));
        float bridgeHeight = (upperNumber >> MapConstants.BridgeHeightBitShift) + MapConstants.MinLandHeight;

        BridgeType bridgeType = _gameData.Get<BridgeTypeSettings>(_gs.ch).Get ((int)objectId);
        if (bridgeType == null)
        {
            return false;
        }

        string prefabName = bridgeType.Art;

        DownloadObjectData dlo = new DownloadObjectData();
        dlo.gameItem = bridgeType;
        dlo.url = prefabName;
        dlo.loadData = loadData;
        dlo.x = x;
        dlo.y = y;
        dlo.finalZ = bridgeHeight;
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategoryNames.Props;

        dlo.rotation = new MyPointF(0, angle, 0);
        dlo.AfterLoad = AfterLoadObject;

        _assetService.LoadAsset(AssetCategoryNames.Props, dlo.url, OnDownloadObject, dlo, null, token);

        return true;
    }
    public void AfterLoadObject(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        if (dlo.rotation != null)
        {
            go.transform.Rotate(dlo.rotation.X, dlo.rotation.Y, dlo.rotation.Z);
        }
    }
}
