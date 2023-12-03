
using GEntity = UnityEngine.GameObject;

using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;

public class FenceObjectLoader : BaseObjectLoader
{
    public FenceObjectLoader(UnityGameState gs) : base(gs)
    {
    }
    public override bool LoadObject(UnityGameState gs, PatchLoadData loadData, uint objectId,
        int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {

        int angleData = (int)(objectId >> 16);
        objectId = (objectId % (1 << 16)) % MapConstants.MapObjectOffsetMult;


        int angle = 0;
        int hangle = 0;
        angle = (4 * (angleData & (255)) - 360);
        hangle = ((4 * (angleData >> 8)) - 360);

        FenceType fenceType = gs.data.GetGameData<FenceTypeSettings>(gs.ch).GetFenceType((int)objectId);
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

        dlo.rotation = new MyPointF(0, angle, hangle);
        dlo.AfterLoad = AfterLoadObject;

        _assetService.LoadAsset(gs, AssetCategoryNames.Props, dlo.url, OnDownloadObject, dlo, null, token);

        return true;
    }
    public void AfterLoadObject(UnityGameState gs, GEntity go, DownloadObjectData dlo, CancellationToken token)
    {
        go.transform().localScale = GVector3.onePlatform;
        go.transform().localRotation = GQuaternion.identity;
        go.transform().position += GVector3.upPlatform;
        if (dlo.rotation != null)
        {
            go.transform().Rotate(dlo.rotation.X, dlo.rotation.Y, dlo.rotation.Z);
        }
    }
}
    