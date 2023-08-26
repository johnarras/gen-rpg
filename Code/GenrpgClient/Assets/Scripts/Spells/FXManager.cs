using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spells.Messages;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FullFX
{
    public FX fx;
    public MapObject from;
    public MapObject to;
    public GameObject fromObj;
    public GameObject toObj;
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
            fromObj = from?.Controller?.gameObject,
            toObj = to?.Controller?.gameObject,
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
        GameObject go = obj as GameObject;

        if (go == null)
        {
            return;
        }
        
        FullFX full = data as FullFX;
        if (full ==null || !TokenUtils.IsValid(full.token))
        {
            GameObject.Destroy(go);
            return;
        }

        LineRenderer line = go.GetComponent<LineRenderer>();
        if (line != null)
        {  
            MapLine wl = GameObjectUtils.GetOrAddComponent<MapLine>(gs,go);
            wl.Init(gs, line, new List<GameObject>() { full.fromObj, full.toObj }, full.fx.Dur, Vector3.zero, token);
        }
        else
        {
            MapProjectile proj = GameObjectUtils.GetOrAddComponent<MapProjectile>(gs,go);

            proj.Init(full,token);
        }
    }
}