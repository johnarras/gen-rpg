using UnityEngine;
using System.Threading;
using Genrpg.Shared.MapServer.Services;
using System.Threading.Tasks;
using Genrpg.Shared.Client.Assets.Constants;

public class MapScreen : BaseScreen
{

    public GameObject ArrowParent = null;
    public GRawImage MapImage;
    private IPlayerManager _playerManager;
    private IMapProvider _mapProvider;
    protected IMapGenData _md;

    GameObject ArrowObject = null;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        Setup();

        await Task.CompletedTask;
    }

    private void Setup()
    {
        _assetService.LoadAssetInto(ArrowParent, AssetCategoryNames.UI, "PlayerArrow", OnLoadArrow, null, _token, Subdirectory);

        _uiService.SetImageTexture(MapImage, MinimapUI.GetTexture());
    }

    private void OnLoadArrow(object obj, object data, CancellationToken token)
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

        // Show player on map with arrow.
        GameObject player = _playerManager.GetPlayerGameObject();
        if (player == null)
        {
            return;
        }

        Vector3 pos = player.transform.localPosition;

        if (MapImage == null)
        {
            return;
        }

        // Player pct goes from -0.5 to 0.5.
        float xpct = pos.x / _mapProvider.GetMap().GetHwid() - 0.5f;
        float ypct = pos.z / _mapProvider.GetMap().GetHhgt() - 0.5f;

        float rot = player.transform.eulerAngles.y;

        float imageSize = MapImage.rectTransform.sizeDelta.x;

        float sx = xpct * imageSize;
        float sy = ypct * imageSize;

        Vector3 cpos = arrow.transform.localPosition;
        arrow.transform.localPosition = new Vector3(sx, sy, cpos.z);

        arrow.transform.eulerAngles = new Vector3(0, 0, -rot);


    }

    protected override void OnStartClose()
    {
    }
}

