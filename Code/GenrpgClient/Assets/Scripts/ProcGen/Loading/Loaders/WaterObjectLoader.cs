
using UnityEngine;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Utils.Data;
using System.Threading;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Client.Assets.Constants;

public class WaterObjectLoader : BaseObjectLoader
{
    public override bool LoadObject(PatchLoadData loadData, uint objectId,
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

        _assetService.LoadAsset(AssetCategoryNames.Prefabs, artName, OnDownloadWater, dlo, null, token);

        return true;

    }
    public virtual void OnDownloadWater(object obj, object data, CancellationToken token)
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

        GameObject go = obj as GameObject;
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
            _clientEntityService.Destroy(go);
            return;
        }

        Terrain terr = dlo.loadData.patch.terrain as Terrain;
        if (terr != null)
        {
            _clientEntityService.AddToParent(go, terr.gameObject);
        }
        else
        {
            _clientEntityService.Destroy(go);
            return;
        }

        float mult = 2.0f; // = 100.0f // if AQUAS
        go.transform.localPosition = new Vector3(dlo.x, dlo.finalZ, dlo.y);
        go.transform.localScale = new Vector3(size.X*mult, 1, size.Z*mult);
        _clientEntityService.SetLayer(go, LayerUtils.NameToLayer(LayerNames.Water));




    }
}