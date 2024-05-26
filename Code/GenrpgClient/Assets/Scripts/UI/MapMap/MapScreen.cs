using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.DataStores.Entities;
using Cysharp.Threading.Tasks;
using System.Threading;
using UI.Screens.Constants;
using Genrpg.Shared.Trades.Messages;

public class MapScreen : BaseScreen
{

    public GEntity ArrowParent = null;
    public GRawImage MapImage;
    private IPlayerManager _playerManager;

    GEntity ArrowObject = null;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        Setup();
        await UniTask.CompletedTask;
    }

    private void Setup()
    {
        _assetService.LoadAssetInto(_gs, ArrowParent, AssetCategoryNames.UI, "PlayerArrow", OnLoadArrow, null, _token, Subdirectory);

        _uIInitializable.SetImageTexture(MapImage, UnityZoneGenService.mapTexture);
    }

    private void OnLoadArrow(UnityGameState gs, object obj, object data, CancellationToken token)
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

        if (_gs.md == null)
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
        float xpct = pos.x / _gs.map.GetHwid() - 0.5f;
        float ypct = pos.z / _gs.map.GetHhgt() - 0.5f;

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

