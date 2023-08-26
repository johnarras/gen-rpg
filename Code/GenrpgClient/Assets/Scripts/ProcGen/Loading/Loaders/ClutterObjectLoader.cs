using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;

public class ClutterObjectLoader : BaseObjectLoader
{
    public ClutterObjectLoader(UnityGameState gs) : base(gs)
    {
    }
    public override bool LoadObject(UnityGameState gs, PatchLoadData loadData, uint objectId,
       int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {
        uint clutterId = objectId - MapConstants.ClutterObjectOffset;

        ClutterType ctype = gs.data.GetGameData<ProcGenSettings>().GetClutterType (clutterId);
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
        dlo.zOffset = 1.0f * (1 + (indexHash + 17) % 3);
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategory.Props;

        dlo.rotation = new MyPointF(((indexHash * 37) % 4) * 90, (indexHash * 23) % 360, ((indexHash * 59) % 4) * 90);

        _assetService.LoadAsset(gs, AssetCategory.Props, dlo.url, OnDownloadObject, dlo, null, token);

        if (indexHash % 3 == 2)
        {
            indexHash = indexHash * 17 + 7;
            dlo = new DownloadObjectData();
            dlo.gameItem = ctype;
            dlo.url = prefabName;
            dlo.loadData = loadData;

            dlo.x = x + ((indexHash / 7) % 3 - 1);
            dlo.y = y + ((indexHash / 131) % 3 - 1);
            dlo.zOffset = 1.0f * (1 + (indexHash + 17) % 3);
            dlo.zone = currZone;
            dlo.zoneType = currZoneType;
            dlo.assetCategory = AssetCategory.Props;
            dlo.AfterLoad = AfterLoadObject;

            dlo.rotation = new MyPointF(((indexHash * 37) % 4) * 90, (indexHash * 23) % 360, ((indexHash * 59) % 4) * 90);

            _assetService.LoadAsset(gs, AssetCategory.Props, dlo.url, OnDownloadObject, dlo, null, token);

           
        }
        return true;
    }

    public void AfterLoadObject(UnityGameState gs, GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        float ddscale = 0.5f;
        go.transform.localPosition = new Vector3(dlo.x + dlo.ddx * ddscale, dlo.height + dlo.zOffset, dlo.y + dlo.ddy * ddscale);
        if (dlo.rotation != null)
        {
            go.transform.eulerAngles = new Vector3(dlo.rotation.X, dlo.rotation.Y, dlo.rotation.Z);
        }
    }
}
