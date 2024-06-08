using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.DataStores.Entities;
using Cysharp.Threading.Tasks;
using System.Threading;
using UI.Screens.Constants;
using Genrpg.Shared.Trades.Messages;
using Genrpg.Shared.MapServer.Services;

public class MapScreen : BaseScreen
{

    public GEntity ArrowParent = null;
    public GRawImage MapImage;
    private IPlayerManager _playerManager;
    private IMapProvider _mapProvider;
    protected IMapGenData _md;

    GEntity ArrowObject = null;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        Setup();
        await UniTask.CompletedTask;
    }

    private void Setup()
    {
        _assetService.LoadAssetInto(ArrowParent, AssetCategoryNames.UI, "PlayerArrow", OnLoadArrow, null, _token, Subdirectory);

        _uIInitializable.SetImageTexture(MapImage, MinimapUI.GetTexture());
    }

    private void OnLoadArrow(object obj, object data, CancellationToken token)
    {
        ArrowObject = obj as GEntity;
        ShowPlayer();
    }

    protected override void ScreenUpdate()
    {
        ShowPlayer();
        base.ScreenUpdate();
    }


    void ShowPlayer()
    {
        GEntity arrow = ArrowObject;
        if (arrow == null)
        {
            return;
        }

        // Show player on map with arrow.
        GEntity player = _playerManager.GetEntity();
        if (player == null)
        {
            return;
        }

        GVector3 pos = GVector3.Create(player.transform().localPosition);

        if (MapImage == null)
        {
            return;
        }

        // Player pct goes from -0.5 to 0.5.
        float xpct = pos.x / _mapProvider.GetMap().GetHwid() - 0.5f;
        float ypct = pos.z / _mapProvider.GetMap().GetHhgt() - 0.5f;

        float rot = player.transform().eulerAngles.y;

        float imageSize = MapImage.rectTransform.sizeDelta.x;

        float sx = xpct * imageSize;
        float sy = ypct * imageSize;

        GVector3 cpos = GVector3.Create(arrow.transform().localPosition);
        arrow.transform().localPosition = GVector3.Create(sx, sy, cpos.z);

        arrow.transform().eulerAngles = GVector3.Create(0, 0, -rot);


    }

    protected override void OnStartClose()
    {
    }
}

