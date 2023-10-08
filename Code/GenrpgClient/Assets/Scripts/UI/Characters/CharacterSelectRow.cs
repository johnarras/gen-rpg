using Genrpg.Shared.Characters.Entities;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.MapServer.Entities;
using Assets.Scripts.Atlas.Constants;
using System.Threading;
using Genrpg.Shared.Login.Messages.DeleteChar;

public class CharacterSelectRow : BaseBehaviour
{
    
    public GText NameText;
    public GImage CharImage;
    public GEntity PlayButtonAnchor;
    public GButton DeleteButton;
    
    public CharacterPlayButton _playButtonPrefab;

    private CharacterSelectScreen _screen;

    private CharacterStub _stub;

    protected CancellationToken _token;
    public void Init(CharacterStub ch, CharacterSelectScreen screenIn, CancellationToken token)
    {
        _stub = ch;
        _screen = screenIn;
        _token = token;
        UIHelper.SetText(NameText, ch.Name);
        UIHelper.SetButton(DeleteButton, screenIn.GetAnalyticsName(), ClickDelete);
        _assetService.LoadSpriteInto(_gs, AtlasNames.Icons, "HelmetMetal_002", CharImage, token);

        if (PlayButtonAnchor == null || _playButtonPrefab == null)
        {
            return;
        }

        foreach (MapStub stub in _gs.mapStubs)
        {
            CharacterPlayButton newButton = GEntityUtils.FullInstantiate<CharacterPlayButton>(_gs, _playButtonPrefab);
            GEntityUtils.AddToParent(newButton.entity(), PlayButtonAnchor);
            newButton.Init(ch.Id, stub.Id, screenIn);
        }
    }

    public CharacterStub GetStub()
    {
        return _stub;
    }

    public void ClickDelete()
    {
        if (_stub == null)
        {
            return;
        }

        DeleteCharCommand com = new DeleteCharCommand()
        {
            CharId = _stub.Id,
        };

        _networkService.SendClientWebCommand(com, _token);
    }
}
