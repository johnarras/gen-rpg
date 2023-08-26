using UnityEngine.UI;
using Genrpg.Shared.Characters.Entities;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using UnityEngine;
using System.Collections.Generic;
using Genrpg.Shared.MapServer.Entities;
using Assets.Scripts.Atlas.Constants;
using System.Threading;
using Genrpg.Shared.Login.Messages.DeleteChar;

public class CharacterSelectRow : BaseBehaviour
{
    [SerializeField]
    private Text _name;

    [SerializeField]
    private Image _charImage;

    [SerializeField]
    private GameObject _playButtonAnchor;

    [SerializeField]
    private Button _deleteButton;

    [SerializeField]
    private CharacterPlayButton _playButtonPrefab;

    private CharacterSelectScreen _screen;

    private CharacterStub _stub;

    protected CancellationToken _token;
    public void Init(CharacterStub ch, CharacterSelectScreen screenIn, CancellationToken token)
    {
        _stub = ch;
        _screen = screenIn;
        _token = token;
        UIHelper.SetText(_name, ch.Name);
        UIHelper.SetButton(_deleteButton, screenIn.GetAnalyticsName(), ClickDelete);
        _assetService.LoadSpriteInto(_gs, AtlasNames.Icons, "HelmetMetal_002", _charImage, token);

        if (_playButtonAnchor == null || _playButtonPrefab == null)
        {
            return;
        }

        foreach (MapStub stub in _gs.mapStubs)
        {
            CharacterPlayButton newButton = GameObjectUtils.FullInstantiate<CharacterPlayButton>(_gs, _playButtonPrefab);
            GameObjectUtils.AddToParent(newButton.gameObject, _playButtonAnchor);
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
