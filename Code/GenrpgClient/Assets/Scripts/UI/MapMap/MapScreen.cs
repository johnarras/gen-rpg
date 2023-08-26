using UnityEngine;
using UnityEngine.UI;
using Genrpg.Shared.DataStores.Entities;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MapScreen : BaseScreen
{

    [SerializeField]
    private GameObject _arrowParent = null;
    [SerializeField]
    private RawImage _mapImage;
    [SerializeField]
    private Button _closeButton;

    GameObject ArrowObject = null;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        UIHelper.SetButton(_closeButton, GetAnalyticsName(), StartClose);
        Setup();
        await UniTask.CompletedTask;
    }

    private void Setup()
    {
        _assetService.LoadAssetInto(_gs, _arrowParent, AssetCategory.UI, "PlayerArrow", OnLoadArrow, null, _token);

        UIHelper.SetImageTexture(_mapImage, UnityZoneGenService.mapTexture);
    }

    private void OnLoadArrow(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        ArrowObject = obj as GameObject;
        ShowPlayer();
    }

    protected override void ScreenUpdate()
    {
        ShowPlayer();
        base.ScreenUpdate();
    }


    void ShowPlayer()
    {
        GameObject arrow = ArrowObject;
        if (arrow == null)
        {
            return;
        }

        if (_gs.md == null)
        {
            return;
        }


        // Show player on map with arrow.
        GameObject player = PlayerObject.Get();
        if (player == null)
        {
            return;
        }

        Vector3 pos = player.transform.localPosition;

        if (_mapImage == null)
        {
            return;
        }

        // Player pct goes from -0.5 to 0.5.
        float xpct = pos.x / _gs.map.GetHwid() - 0.5f;
        float ypct = pos.z / _gs.map.GetHhgt() - 0.5f;

        float rot = player.transform.eulerAngles.y;

        float imageSize = _mapImage.rectTransform.sizeDelta.x;

        float sx = xpct * imageSize;
        float sy = ypct * imageSize;

        Vector3 cpos = arrow.transform.localPosition;
        arrow.transform.localPosition = new Vector3(sx, sy, cpos.z);

        arrow.transform.eulerAngles = new Vector3(0, 0, -rot);


    }

}

