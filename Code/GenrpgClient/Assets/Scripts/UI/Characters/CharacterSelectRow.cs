using Genrpg.Shared.Characters.PlayerData;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.MapServer.Entities;
using Assets.Scripts.Atlas.Constants;
using System.Threading;
using Genrpg.Shared.Login.Messages.DeleteChar;

public class CharacterSelectRow : BaseBehaviour
{
    private IWebNetworkService _webNetworkService;

    public GText NameText;
    public GImage CharImage;
    public GEntity PlayButtonAnchor;
    public GButton DeleteButton;
    
    private CharacterStub _characterStub;
    private CharacterSelectScreen _screen;

    protected CancellationToken _token;
    public void Init(CharacterStub ch, CharacterSelectScreen screen, CancellationToken token)
    {
        _screen = screen;
        _characterStub = ch;
        _token = token;
        UIHelper.SetText(NameText, ch.Name);
        UIHelper.SetButton(DeleteButton, screen.GetAnalyticsName(), ClickDelete);
        _assetService.LoadSpriteInto(_gs, AtlasNames.Icons, "HelmetMetal_002", CharImage, token);

        if (PlayButtonAnchor == null)
        {
            return;
        }

        foreach (MapStub stub in _gs.mapStubs)
        {
            _assetService.LoadAssetInto(_gs, PlayButtonAnchor, AssetCategoryNames.UI, 
                "CharacterPlayButton", OnDownloadPlayButton, stub, token, screen.Subdirectory);          
        }
    }

    public CharacterStub GetStub()
    {
        return _characterStub;
    }

    public void ClickDelete()
    {
        if (_characterStub == null)
        {
            return;
        }

        DeleteCharCommand com = new DeleteCharCommand()
        {
            CharId = _characterStub.Id,
        };

        _webNetworkService.SendClientWebCommand(com, _token);
    }

    private void OnDownloadPlayButton(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;

        if (go == null)
        {
            return;
        }

        MapStub stub = data as MapStub;

        if (stub == null)
        {
            Destroy(go);
        }

        CharacterPlayButton button = go.GetComponent<CharacterPlayButton>();
        if (button == null)
        {
            Destroy(go);
        }

        button.Init(_characterStub.Id, stub.Id, _screen);
    }
}
