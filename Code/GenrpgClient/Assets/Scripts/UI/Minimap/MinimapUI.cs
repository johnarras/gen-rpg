
using System;
using System.Collections;
using GEntity = UnityEngine.GameObject;
using ClientEvents;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using UnityEngine;

public class MinimapUI : BaseBehaviour
{
    
    public GEntity MainPanel;
    public GRawImage MapImage;
    public GEntity ArrowParent;
    public GEntity ArrowObject;

    private CancellationToken _token;
        
    public void Init(CancellationToken token)
    {
        _token = token;
        _gs.AddEvent<EnableMinimapEvent>(this, OnEnableMinimap);
        _gs.AddEvent<DisableMinimapEvent>(this, OnDisableMinimap);
        _gs.AddEvent<LoadMinimapEvent>(this, OnLoadMinimap);
        OnDisableMinimap(_gs, null);
        AddUpdate(MinimapUpdate, UpdateType.Regular);
        if (ArrowParent != null)
        {
            _assetService.LoadAssetInto(_gs, ArrowParent, AssetCategoryNames.UI, "PlayerArrow", OnLoadArrow, null, token);
        }

        ShowMapImage();
    }

    private void ShowMapImage()
    {
        if (MapImage != null && UnityZoneGenService.mapTexture != null)
        {
            UIHelper.SetImageTexture(MapImage, UnityZoneGenService.mapTexture);
            OnEnableMinimap(_gs, null);
            
        }
        else
        {
            OnDisableMinimap(_gs, null);
        }
    }

    private LoadMinimapEvent OnLoadMinimap (UnityGameState gs, LoadMinimapEvent data)
    {
      
        ShowMapImage();
        return null;
    }

    private void OnLoadArrow(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        ArrowObject = obj as GEntity;
    }

	void MinimapUpdate()
	{
        GEntity player = PlayerObject.Get ();
        if (player == null || MapImage ==  null || MapImage.texture == null || _gs.md == null || _gs.md.GeneratingMap)
        {
            return;
        }
        GVector3 pos = GVector3.Create(player.transform().localPosition);

        // Player pct goes from -0.5 to 0.5.
        float xpct = pos.x / _gs.map.GetHwid();
        float ypct = pos.z / _gs.map.GetHhgt();

        float imageSize = MapImage.rectTransform.sizeDelta.x;


        if (MapImage.texture != null)
        {
            float sizePct = 256.0f / _gs.map.GetHwid();
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

    private EnableMinimapEvent OnEnableMinimap(UnityGameState gs, EnableMinimapEvent data)
    {
        GEntityUtils.SetActive(MainPanel, true);
        return null;
    }

    private DisableMinimapEvent OnDisableMinimap(UnityGameState gs, DisableMinimapEvent data)
    {
        GEntityUtils.SetActive(MainPanel, false);
        return null;
    }

    

}

