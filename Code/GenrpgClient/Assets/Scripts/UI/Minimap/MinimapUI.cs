
using System;
using System.Collections;
using GEntity = UnityEngine.GameObject;
using ClientEvents;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.MapServer.Entities.MapCache;

public class MinimapUI : BaseBehaviour
{
    
    public GEntity MainPanel;
    public GRawImage MapImage;
    public GEntity ArrowParent;
    public GEntity ArrowObject;

    private CancellationToken _token;
    private IPlayerManager _playerManager;
    private IMapProvider _mapProvider;
    protected IMapGenData _md;

    public void Init(CancellationToken token)
    {
        _token = token;
        _dispatcher.AddEvent<EnableMinimapEvent>(this, OnEnableMinimap);
        _dispatcher.AddEvent<DisableMinimapEvent>(this, OnDisableMinimap);
        OnDisableMinimap(null);
        AddUpdate(MinimapUpdate, UpdateType.Regular);
        if (ArrowParent != null)
        {
            _assetService.LoadAssetInto(ArrowParent, AssetCategoryNames.UI, "PlayerArrow", OnLoadArrow, null, token, "Maps");
        }

        ShowMapImage();
    }

    private void ShowMapImage()
    {
        if (_mapTexture != null && MapImage != null)
        {
            _uIInitializable.SetImageTexture(MapImage, _mapTexture);
            OnEnableMinimap(null);
            
        }
        else
        {
            OnDisableMinimap(null);
        }
    }


    private static Texture2D _mapTexture = null;
    public static void SetTexture(Texture2D tex)
    {
        if (tex == null && _mapTexture != null)
        {
            Resources.UnloadAsset(_mapTexture);
        }

        _mapTexture = tex;
        if (_mapTexture != null)
        {
            _mapTexture.wrapMode = TextureWrapMode.Clamp;
        }
    }

    public static Texture2D GetTexture()
    {
        return _mapTexture;
    }

    private void OnLoadArrow(object obj, object data, CancellationToken token)
    {
        ArrowObject = obj as GEntity;
    }

	void MinimapUpdate()
	{
        GEntity player = _playerManager.GetEntity ();
        if (player == null || MapImage ==  null || MapImage.texture == null || _md.GeneratingMap)
        {
            return;
        }
        GVector3 pos = GVector3.Create(player.transform().localPosition);

        // Player pct goes from -0.5 to 0.5.
        float xpct = pos.x / _mapProvider.GetMap().GetHwid();
        float ypct = pos.z / _mapProvider.GetMap().GetHhgt();

        float imageSize = MapImage.rectTransform.sizeDelta.x;


        if (MapImage.texture != null)
        {
            float sizePct = 256.0f / _mapProvider.GetMap().GetHwid();
            sizePct = MathUtils.Clamp(0.02f, sizePct, 0.15f);
            float xminpct = xpct - sizePct / 2;
            float yminpct = ypct - sizePct / 2;
            MapImage.uvRect = new Rect(new Vector2(xminpct, yminpct), new Vector2(sizePct, sizePct));
        }


        float rot = player.transform().eulerAngles.y;

		if (ArrowObject != null)
		{
			ArrowObject.transform().eulerAngles = GVector3.Create(0,0,-rot);
		}



	}

    private void OnEnableMinimap(EnableMinimapEvent data)
    {
        GEntityUtils.SetActive(MainPanel, true);
        return;
    }

    private void OnDisableMinimap(DisableMinimapEvent data)
    {
        GEntityUtils.SetActive(MainPanel, false);
        return;
    }

    

}

