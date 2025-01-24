using Genrpg.Shared.Characters.PlayerData;
using UnityEngine;
using Genrpg.Shared.MapServer.Entities;
using System.Threading;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Characters.WebApi.DeleteChar;

public class CharacterSelectRow : BaseBehaviour
{
    private IClientWebService _webNetworkService;

    public GText NameText;
    public GImage CharImage;
    public GameObject PlayButtonAnchor;
    public GButton DeleteButton;
    
    private CharacterStub _characterStub;
    private CharacterSelectScreen _screen;

    protected CancellationToken _token;
    public void Init(CharacterStub ch, CharacterSelectScreen screen, CancellationToken token)
    {
        _screen = screen;
        _characterStub = ch;
        _token = token;
        _uiService.SetText(NameText, ch.Name);
        _uiService.SetButton(DeleteButton, screen.GetName(), ClickDelete);
        _assetService.LoadAtlasSpriteInto(AtlasNames.Icons, "HelmetMetal_002", CharImage, token);

        if (PlayButtonAnchor == null)
        {
            return;
        }

        foreach (MapStub stub in _gs.mapStubs)
        {
            _assetService.LoadAssetInto(PlayButtonAnchor, AssetCategoryNames.UI, 
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

        DeleteCharRequest com = new DeleteCharRequest()
        {
            CharId = _characterStub.Id,
        };

        _webNetworkService.SendClientUserWebRequest(com, _token);
    }

    private void OnDownloadPlayButton(object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;

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
