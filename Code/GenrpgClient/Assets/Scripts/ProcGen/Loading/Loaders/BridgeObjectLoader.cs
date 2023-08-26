using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Entities;
using System.Threading;
using UnityEngine;

public class BridgeObjectLoader : BaseObjectLoader
{
    public BridgeObjectLoader(UnityGameState gs) :base(gs)
    {
    }
    public override bool LoadObject(UnityGameState gs, PatchLoadData loadData, uint objectId,
       int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {

        uint upperNumber = objectId >> 16;
        objectId = (objectId % (1 << 16)) % MapConstants.MapObjectOffsetMult;


        float btypeId = objectId;
        int angle = (int)(MapConstants.BridgeAngleDiv * (upperNumber & ((1 << MapConstants.BridgeHeightBitShift) - 1)));
        float bridgeHeight = (upperNumber >> MapConstants.BridgeHeightBitShift) + MapConstants.MinLandHeight;

        BridgeType bridgeType = gs.data.GetGameData<ProcGenSettings>().GetBridgeType ((int)objectId);
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
        dlo.assetCategory = AssetCategory.Props;

        dlo.rotation = new MyPointF(0, angle, 0);
        dlo.AfterLoad = AfterLoadObject;

        _assetService.LoadAsset(gs, AssetCategory.Props, dlo.url, OnDownloadObject, dlo, null, token);

        return true;
    }
    public void AfterLoadObject(UnityGameState gs, GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        if (dlo.rotation != null)
        {
            go.transform.Rotate(dlo.rotation.X, dlo.rotation.Y, dlo.rotation.Z);
        }
    }
}
