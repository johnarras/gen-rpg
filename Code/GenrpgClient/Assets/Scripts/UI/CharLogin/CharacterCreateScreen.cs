using UnityEngine.UI;
using Genrpg.Shared.Utils;
using Cysharp.Threading.Tasks;
using UI.Screens.Constants;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.Login.Messages.CreateChar;

public class CharacterCreateScreen : BaseScreen
{
    [SerializeField]
    private InputField _nameInput;

    [SerializeField]
    private Button _createButton;
    [SerializeField]
    private Button _backButton;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        UIHelper.SetButton(_createButton, GetAnalyticsName(), ClickCreate);
        UIHelper.SetButton(_backButton, GetAnalyticsName(),ClickBack);
        await UniTask.CompletedTask;
    }

    public void ClickBack()
    {
        if (!CanClick("back"))
        {
            return;
        }

        _screenService.Open(_gs, ScreenId.CharacterSelect);
        _screenService.Close(_gs, ScreenId.CharacterCreate);
    }
    public void ClickCreate()
    {
        if (!CanClick("create"))
        {
            return;
        }

        string charName = UIHelper.GetInputText(_nameInput);
        if (string.IsNullOrEmpty(charName))
        {
            _gs.logger.Message("You need to choose a name!");
            return;
        }

        CreateCharCommand createCommand = new CreateCharCommand()
        {
            Name = charName,
        };

        _networkService.SendClientWebCommand(createCommand, _token);

    }
}

