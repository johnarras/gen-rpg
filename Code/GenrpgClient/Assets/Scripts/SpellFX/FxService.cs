using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spells.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

public interface IFxService : IInitializable
{
    void ShowFX(FX fx, CancellationToken token);
}

public class FxService : IFxService
{
    private IClientMapObjectManager _objectManager;
    private IAssetService _assetService;
    private IUnityGameState _gs;
    protected IGameObjectService _gameObjectService;

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }


    public void ShowFX(FX fx, CancellationToken token)
    {
        if (!_objectManager.GetGridItem(fx.From, out ClientMapObjectGridItem from))
        {
            return;
        }
        if (!_objectManager.GetGridItem(fx.To, out ClientMapObjectGridItem to))
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

        _assetService.LoadAsset(AssetCategoryNames.Magic, fx.Art, OnLoadFX, full, null, token);
    }

    private void OnLoadFX(object obj, object data, CancellationToken token)
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
        MapProjectile proj = _gameObjectService.GetOrAddComponent<MapProjectile>(go);

        proj.Init(full, token);

    }
}