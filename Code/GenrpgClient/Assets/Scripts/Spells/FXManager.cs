using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spells.Messages;
using System.Collections.Generic;
using System.Threading;
using UnityEngine; // Needed
using GEntity = UnityEngine.GameObject;

public class FullFX
{
    public FX fx;
    public MapObject from;
    public MapObject to;
    public GEntity fromObj;
    public GEntity toObj;
    public CancellationToken token;

}
public class FXManager
{
    public static void ShowFX(UnityGameState gs, FX fx, IClientMapObjectManager om, IAssetService assetService,
        CancellationToken token)
    {
        if (!om.GetGridItem(fx.From, out ClientMapObjectGridItem from))
        {
            return;
        }
        if (!om.GetGridItem(fx.To, out ClientMapObjectGridItem to))
        {
            return;
        }

        FullFX full = new FullFX()
        {
            from = from.Obj,
            to = to.Obj,
            fx = fx,
            fromObj = from?.Controller?.entity(),
            toObj = to?.Controller?.entity(),
            token = token,
        };

        if (full.fromObj == null || full.toObj == null)
        {
            return;
        }

        assetService.LoadAsset(gs, AssetCategory.Magic, fx.Art, OnLoadFX, full, null, token);
    }

    private static void OnLoadFX(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;

        if (go == null)
        {
            return;
        }
        
        FullFX full = data as FullFX;
        if (full ==null || !TokenUtils.IsValid(full.token))
        {
            GEntityUtils.Destroy(go);
            return;
        }
        MapProjectile proj = GEntityUtils.GetOrAddComponent<MapProjectile>(gs, go);

        proj.Init(full, token);

    }
}