using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using Genrpg.Shared.Constants;
using Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using Assets.Scripts.MapTerrain;

public abstract class BaseObjectLoader
{
    protected IAssetService _assetService;
    public BaseObjectLoader(UnityGameState gs)
    {
        gs.loc.Resolve(this);
    }


    public abstract bool LoadObject(UnityGameState gs, PatchLoadData loadData, uint objectId, int x, int y, 
        Zone currZone, ZoneType currZoneType, CancellationToken token);


    protected void OnDownloadObject(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        FinalPlaceObject(gs, obj as GameObject, data as DownloadObjectData, token);
    }

    public virtual void FinalPlaceObject(UnityGameState gs, GameObject go, DownloadObjectData dlo, CancellationToken token)
    {
        if (go == null)
        {
            return;
        }

        if (dlo == null)
        {
            GameObject.Destroy(go);
            return;
        }

        if (dlo == null || dlo.loadData == null || dlo.loadData.patch == null)
        {
            GameObject.Destroy(go);
            return;
        }


        int gx = dlo.loadData.gx;
        int gy = dlo.loadData.gy;
        int wx = gx * (MapConstants.TerrainPatchSize - 1) + dlo.x;
        int wy = gy * (MapConstants.TerrainPatchSize - 1) + dlo.y;

        if (!dlo.allowRandomPlacement)
        {
            wx = dlo.x;
            wy = dlo.y;
        }


        TerrainPatchData patch = dlo.loadData.patch;

        Terrain terr = patch.terrain as Terrain;
        if (terr == null)
        {
            return;
        }

        GameObject terrGo = terr.gameObject;

        if (terrGo == null)
        {
            GameObject.Destroy(go);
            return;
        }

        GameObjectUtils.AddToParent(go, terrGo);
        GameObjectUtils.SetLayer(go, LayerMask.NameToLayer(LayerNames.ObjectLayer));

        dlo.placementSeed = 17041 + dlo.x * 9479 + dlo.y * 2281 + dlo.loadData.gx * 5281 + dlo.loadData.gy * 719
            + dlo.loadData.gx * dlo.y + dlo.loadData.gy * dlo.x;


        if (dlo.allowRandomPlacement)
        {
            dlo.ddx = MathUtils.SeedFloatRange(dlo.placementSeed * 13, 143, -0.5f, 0.5f, 101);
            dlo.ddy = MathUtils.SeedFloatRange(dlo.placementSeed * 17, 149, -0.5f, 0.5f, 101);
        }
        dlo.height = gs.md.SampleHeight(gs, wx, MapConstants.MapHeight, wy);
        go.transform.localPosition = new Vector3(dlo.x + dlo.ddx, dlo.height + dlo.zOffset, dlo.y + dlo.ddy);
        go.transform.localScale = Vector3.one;
        if (dlo.finalZ > 0)
        {
            go.transform.localPosition = new Vector3(dlo.x + dlo.ddx, dlo.finalZ, dlo.y + dlo.ddy);
        }
        if (dlo.rotation != null)
        {
            go.transform.Rotate(dlo.rotation.X, dlo.rotation.Y, dlo.rotation.Z);
        }
        else
        {
            go.transform.Rotate(0, (dlo.placementSeed * 13) % 360, 0);
        }
        if (dlo.AfterLoad != null)
        {
            dlo.AfterLoad(gs, go, dlo, token);
        }

        if (dlo.scale != 1.0f)
        {
            go.transform.localScale = dlo.scale * Vector3.one;
        }
    }



}
