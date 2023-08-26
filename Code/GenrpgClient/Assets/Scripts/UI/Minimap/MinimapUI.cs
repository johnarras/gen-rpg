
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using Genrpg.Shared.Core.Entities;


using Services;
using ClientEvents;
using Genrpg.Shared.DataStores.Entities;
using Entities;
using Genrpg.Shared.Utils;
using System.Threading;

public class MinimapUI : BaseBehaviour
{
    [SerializeField]
    private GameObject _panel;
    [SerializeField]
    private RawImage _mapImage;
    [SerializeField]
    private GameObject _arrowParent;
    private GameObject _arrowObject;

    private CancellationToken _token;
        
    public void Init(CancellationToken token)
    {
        _token = token;
        _gs.AddEvent<EnableMinimapEvent>(this, OnEnableMinimap);
        _gs.AddEvent<DisableMinimapEvent>(this, OnDisableMinimap);
        _gs.AddEvent<LoadMinimapEvent>(this, OnLoadMinimap);
        OnDisableMinimap(_gs, null);
        AddUpdate(MinimapUpdate, UpdateType.Regular);
        if (_arrowParent != null)
        {
            _assetService.LoadAssetInto(_gs, _arrowParent, AssetCategory.UI, "PlayerArrow", OnLoadArrow, null, token);
        }

        ShowMapImage();
    }

    private void ShowMapImage()
    {
        if (_mapImage != null && UnityZoneGenService.mapTexture != null)
        {
            UIHelper.SetImageTexture(_mapImage, UnityZoneGenService.mapTexture);
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
        _arrowObject = obj as GameObject;
    }

	void MinimapUpdate()
	{
        GameObject player = PlayerObject.Get ();
        if (player == null || _mapImage ==  null || _mapImage.texture == null || _gs.md == null || _gs.md.GeneratingMap)
        {
            return;
        }
        Vector3 pos = player.transform.localPosition;

        // Player pct goes from -0.5 to 0.5.
        float xpct = pos.x / _gs.map.GetHwid();
        float ypct = pos.z / _gs.map.GetHhgt();

        float imageSize = _mapImage.rectTransform.sizeDelta.x;


        if (_mapImage.texture != null)
        {
            float sizePct = 256.0f / _gs.map.GetHwid();
            sizePct = MathUtils.Clamp(0.02f, sizePct, 0.15f);
            float xminpct = xpct - sizePct / 2;
            float yminpct = ypct - sizePct / 2;
            _mapImage.uvRect = new Rect(new Vector2(xminpct, yminpct), new Vector2(sizePct, sizePct));
        }


        float rot = player.transform.eulerAngles.y;

		if (_arrowObject != null)
		{
			_arrowObject.transform.eulerAngles = new Vector3(0,0,-rot);
		}



	}

    private EnableMinimapEvent OnEnableMinimap(UnityGameState gs, EnableMinimapEvent data)
    {
        GameObjectUtils.SetActive(_panel, true);
        return null;
    }

    private DisableMinimapEvent OnDisableMinimap(UnityGameState gs, DisableMinimapEvent data)
    {
        GameObjectUtils.SetActive(_panel, false);
        return null;
    }

    

}

