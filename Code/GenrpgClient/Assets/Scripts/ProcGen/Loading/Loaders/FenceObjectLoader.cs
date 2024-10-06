
using UnityEngine;

using Genrpg.Shared.Utils.Data;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Fences;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Client.Assets.Constants;

public class FenceObjectLoader : BaseObjectLoader
{

    private ILogService _logService;
    public override bool LoadObject(PatchLoadData loadData, uint objectId,
        int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {

        int angleData = (int)(objectId >> 16);
        objectId = (objectId % (1 << 16)) % MapConstants.MapObjectOffsetMult;


        int angle = 0;
        int hangle = 0;
        angle = (4 * (angleData & (255)) - 360);
        hangle = ((4 * (angleData >> 8)) - 360);

        FenceType fenceType = _gameData.Get<FenceTypeSettings>(_gs.ch).Get((int)objectId);
        if (fenceType == null)
        {
            return false;
        }

        string artName = fenceType.Art;

        DownloadObjectData dlo = new DownloadObjectData();
        dlo.gameItem = fenceType;
        dlo.url = artName;
        dlo.loadData = loadData;
        dlo.x = x;
        dlo.y = y;
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategoryNames.Props;
        dlo.allowRandomPlacement = false;
        dlo.rotation = new MyPointF(0, angle, hangle);
        dlo.AfterLoad = AfterLoadObject;
        
        _assetService.LoadAsset(AssetCategoryNames.Props, dlo.url, OnDownloadObject, dlo, null, token);

        return true;
    }
    public void AfterLoadObject(GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localPosition += Vector3.up;
        if (dlo.rotation != null)
        {
            go.transform.Rotate(dlo.rotation.X, dlo.rotation.Y, dlo.rotation.Z);
        }
    }
}
    