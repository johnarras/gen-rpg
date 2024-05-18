
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Utils;
using System.Threading;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;

public abstract class BaseObjectLoader : IInjectable
{
    protected IAssetService _assetService;
    protected IMapTerrainManager _terrainManager;
    protected IGameData _gameData;

    public abstract bool LoadObject(UnityGameState gs, PatchLoadData loadData, uint objectId, int x, int y, 
        Zone currZone, ZoneType currZoneType, CancellationToken token);

    protected void OnDownloadObject(UnityGameState gs, object obj, object data, CancellationToken token)
    {
        FinalPlaceObject(gs, obj as GEntity, data as DownloadObjectData, token);
    }

    public virtual void FinalPlaceObject(UnityGameState gs, GEntity go, DownloadObjectData dlo, CancellationToken token)
    {
        if (go == null)
        {
            return;
        }

        if (dlo == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        if (dlo == null || dlo.loadData == null || dlo.loadData.patch == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }


        int gx = dlo.loadData.gx;
        int gy = dlo.loadData.gy;
        int wx = gx * (MapConstants.TerrainPatchSize - 1) + dlo.x;
        int wy = gy * (MapConstants.TerrainPatchSize - 1) + dlo.y;

        TerrainPatchData patch = dlo.loadData.patch;

        Terrain terr = patch.terrain as Terrain;
        if (terr == null)
        {
            return;
        }

        GEntity terrGo = terr.entity();

        if (terrGo == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        GEntityUtils.AddToParent(go, terrGo);
        GEntityUtils.SetLayer(go, LayerUtils.NameToLayer(LayerNames.ObjectLayer));

        dlo.placementSeed = 17041 + dlo.x * 9479 + dlo.y * 2281 + dlo.loadData.gx * 5281 + dlo.loadData.gy * 719
            + dlo.loadData.gx * dlo.y + dlo.loadData.gy * dlo.x;


        if (dlo.allowRandomPlacement)
        {
            dlo.ddx = MathUtils.SeedFloatRange(dlo.placementSeed * 13, 143, -0.5f, 0.5f, 101);
            dlo.ddy = MathUtils.SeedFloatRange(dlo.placementSeed * 17, 149, -0.5f, 0.5f, 101);
        }
        dlo.height = _terrainManager.SampleHeight(gs, wx, wy);
        go.transform().localPosition = GVector3.Create(dlo.x + dlo.ddx, dlo.height + dlo.zOffset, dlo.y + dlo.ddy);
        go.transform().localScale = GVector3.onePlatform;
        if (dlo.finalZ > 0)
        {
            go.transform().localPosition = GVector3.Create(dlo.x + dlo.ddx, dlo.finalZ, dlo.y + dlo.ddy);
        }
        if (dlo.rotation != null)
        {
            go.transform().Rotate(dlo.rotation.X, dlo.rotation.Y, dlo.rotation.Z);
        }
        else
        {
            go.transform().Rotate(0, (dlo.placementSeed * 13) % 360, 0);
        }
        if (dlo.AfterLoad != null)
        {
            dlo.AfterLoad(gs, go, dlo, token);
        }

        if (dlo.scale != 1.0f)
        {
            go.transform().localScale = GVector3.Create(GVector3.one * dlo.scale);
        }
    }



}
