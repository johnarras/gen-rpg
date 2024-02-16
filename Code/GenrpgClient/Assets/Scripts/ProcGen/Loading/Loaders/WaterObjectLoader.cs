
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Constants;

using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Entities;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;

public class WaterObjectLoader : BaseObjectLoader
{
    public WaterObjectLoader(UnityGameState gs) : base(gs)
    {
    }
    public override bool LoadObject(UnityGameState gs, PatchLoadData loadData, uint objectId,
        int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {
        uint upperNumber = objectId >> 16;
        objectId = (objectId % (1 << 16)) % MapConstants.MapObjectOffsetMult;

        float heightOffset = 1.0f * objectId + MapConstants.MinLandHeight;
        uint xSize = upperNumber / 256;
        uint zSize = upperNumber % 256;

        string artName = MapConstants.WaterName;
        if (currZone == null)
        {
            artName = MapConstants.MinimapWaterName;
        }

        DownloadObjectData dlo = new DownloadObjectData();
        dlo.url = artName;
        dlo.loadData = loadData;
        dlo.x = x;
        dlo.y = y;
        dlo.finalZ = heightOffset-0.5f;
        dlo.zone = currZone;
        dlo.zoneType = currZoneType;
        dlo.assetCategory = AssetCategoryNames.Prefabs;
        dlo.data = new MyPointF(xSize, heightOffset, zSize);

        _assetService.LoadAsset(gs, AssetCategoryNames.Prefabs, artName, OnDownloadWater, dlo, null, token);

        return true;

    }
    public virtual void OnDownloadWater(UnityGameState gs, object obj, object data, CancellationToken token)
    {
        DownloadObjectData dlo = data as DownloadObjectData;
        if (dlo == null)
        {
            return;
        }

        MyPointF size = dlo.data as MyPointF;
        if (size == null)
        {
            return;
        }

        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        int gx = dlo.loadData.gx;
        int gy = dlo.loadData.gy;
        int wx = gx * (MapConstants.TerrainPatchSize - 1) + dlo.x;
        int wy = gy * (MapConstants.TerrainPatchSize - 1) + dlo.y;

        if (dlo.loadData.patch == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        Terrain terr = dlo.loadData.patch.terrain as Terrain;
        if (terr != null)
        {
            GEntityUtils.AddToParent(go, terr.entity());
        }
        else
        {
            GEntityUtils.Destroy(go);
            return;
        }

        float mult = 2.0f; // = 100.0f // if AQUAS
        go.transform().localPosition = GVector3.Create(dlo.x, dlo.finalZ, dlo.y);
        go.transform().localScale = GVector3.Create(size.X*mult, 1, size.Z*mult);
        GEntityUtils.SetLayer(go, LayerUtils.NameToLayer(LayerNames.Water));




    }
}