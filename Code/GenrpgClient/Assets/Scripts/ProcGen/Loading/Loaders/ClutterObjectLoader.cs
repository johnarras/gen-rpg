
using GEntity = UnityEngine.GameObject;

using Genrpg.Shared.Utils.Data;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.Utils;
using Genrpg.Shared.ProcGen.Settings.Clutter;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Zones.Settings;

public class ClutterObjectLoader : BaseObjectLoader
{
    public override bool LoadObject(UnityGameState gs, PatchLoadData loadData, uint objectId,
       int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {
        uint clutterId = objectId - MapConstants.ClutterObjectOffset;

        ClutterType ctype = _gameData.Get<ClutterTypeSettings>(gs.ch).Get(clutterId);
        if (ctype == null)
        {
            return false;
        }

        string artName = ctype.Art;
        int indexHash = loadData.gx * y + loadData.gy * x + x * 13 + loadData.gy * 19 + loadData.gx * 31 + y * 47;
        int indexChoice = 1;
        if (ctype.NumChoices > 0)
        {
            indexChoice = 1 + indexHash % ctype.NumChoices;
        }
        string prefabName = ctype.Art + indexChoice;

        DownloadObjectData dlo = new DownloadObjectData();
        dlo.gameItem = ctype;
        dlo.url = prefabName;
        dlo.loadData = loadData;
        dlo.x = x;
        dlo.y = y;
        dlo.zOffset = MathUtils.FloatRange(0, 1, gs.rand);
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategoryNames.Props;

        dlo.rotation = new MyPointF(((indexHash * 37) % 4) * 90, (indexHash * 23) % 360, ((indexHash * 59) % 4) * 90);

        _assetService.LoadAsset(gs, AssetCategoryNames.Props, dlo.url, OnDownloadObject, dlo, null, token);

        if (indexHash % 3 == 2)
        {
            indexHash = indexHash * 17 + 7;
            dlo = new DownloadObjectData();
            dlo.gameItem = ctype;
            dlo.url = prefabName;
            dlo.loadData = loadData;

            dlo.x = x + ((indexHash / 7) % 3 - 1);
            dlo.y = y + ((indexHash / 131) % 3 - 1);
            dlo.zOffset = MathUtils.FloatRange(0, 1, gs.rand);
            dlo.zone = currZone;
            dlo.zoneType = currZoneType;
            dlo.assetCategory = AssetCategoryNames.Props;
            dlo.AfterLoad = AfterLoadObject;

            dlo.rotation = new MyPointF(((indexHash * 37) % 4) * 90, (indexHash * 23) % 360, ((indexHash * 59) % 4) * 90);

            _assetService.LoadAsset(gs, AssetCategoryNames.Props, dlo.url, OnDownloadObject, dlo, null, token);

           
        }
        return true;
    }

    public void AfterLoadObject(UnityGameState gs, GEntity go, DownloadObjectData dlo, CancellationToken token)
    {
        float ddscale = 0.5f;

        MeshCollider collider = go.GetComponent<MeshCollider>();

        if (collider != null)
        { 
            collider.convex = true;
        }

        go.transform().localPosition = GVector3.Create(dlo.x + dlo.ddx * ddscale, dlo.height + dlo.zOffset, dlo.y + dlo.ddy * ddscale);
        if (dlo.rotation != null)
        {
            go.transform().eulerAngles = GVector3.Create(dlo.rotation.X, dlo.rotation.Y, dlo.rotation.Z);
        }
    }
}
