﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;

public class RockObjectLoader : BaseObjectLoader
{
    public RockObjectLoader(UnityGameState gs) : base(gs)
    {
    }
    public override bool LoadObject(UnityGameState gs, PatchLoadData loadData, uint objectId, 
        int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {
        if (objectId < MapConstants.RockObjectOffset + MapConstants.MapObjectOffsetMult)
        {
            objectId %= MapConstants.RockObjectOffset;
        }

        objectId %= MapConstants.MapObjectOffsetMult;



        RockType rockType = gs.data.GetGameData<ProcGenSettings>().GetRockType(objectId);
        if (rockType == null || rockType.Art == null)
        {
            return false;
        }

        int indexHash = (loadData.gx * 113 + loadData.gy * 317 + x * 59 + y * 3141) % rockType.MaxIndex;

        int index = 0;
        if (rockType.MaxIndex > 0)
        {
            index = (indexHash / 2) % rockType.MaxIndex;
        }

        string artName = rockType.Art + (indexHash.ToString("D3"));

        bool smallObject = ((indexHash * 5) % 3 == 0);

        DownloadObjectData dlo = new DownloadObjectData();
        dlo.gameItem = rockType;
        dlo.url = artName;
        dlo.loadData = loadData;
        dlo.x = x;
        dlo.y = y;
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategory.Rocks;
        dlo.data = (smallObject ? "small" : "");
        dlo.AfterLoad = AfterLoadRock;

        _assetService.LoadAsset(gs, AssetCategory.Rocks, artName, OnDownloadObject, dlo, null, token);

        return true;

    }

    public void AfterLoadRock(UnityGameState gs, GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        if (go == null || dlo == null)
        {
            return;
        }

        float minScale = 0.8f;
        float maxScale = 2.0f;

        if (dlo.data != null && dlo.data.ToString() == "small")
        {
            minScale *= 0.3f;
            maxScale *= 0.6f;

            if (dlo.placementSeed % 17 == 5)
            {
                minScale *= 1.0f;
                maxScale *= 1.4f;
            }
        }

        float newScale = MathUtils.SeedFloatRange(dlo.placementSeed, 147, minScale, maxScale);



        go.transform.localScale = new Vector3(newScale, newScale, newScale);

        float xrot = MathUtils.SeedFloatRange(dlo.placementSeed, 103, 0, 359, 360);
        float yrot = MathUtils.SeedFloatRange(dlo.placementSeed, 461, 0, 359, 360);
        float zrot = MathUtils.SeedFloatRange(dlo.placementSeed, 2767, 0, 359, 360);



        go.transform.Rotate(xrot, yrot, zrot);


        //go.transform.position = new Vector3(dlo.x, go.transform.position.y, dlo.y);
    }
}