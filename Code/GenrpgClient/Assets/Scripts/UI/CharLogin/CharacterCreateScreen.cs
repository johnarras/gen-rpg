using Cysharp.Threading.Tasks;
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Login.Messages.CreateChar;

public class CharacterCreateScreen : BaseScreen
{
    private IWebNetworkService _webNetworkService;
    
    public GInputField NameInput;
    public GButton CreateButton;
    public GButton BackButton;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        _uIInitializable.SetButton(CreateButton, GetName(), ClickCreate);
        _uIInitializable.SetButton(BackButton, GetName(),ClickBack);
        await UniTask.CompletedTask;
    }

    public void ClickBack()
    {
        _screenService.Open(ScreenId.CharacterSelect);
        _screenService.Close(ScreenId.CharacterCreate);
    }
    public void ClickCreate()
    {
        string charName = NameInput.Text;
        if (string.IsNullOrEmpty(charName))
        {
            _logService.Message("You need to choose a name!");
            return;
        }

        CreateCharCommand createCommand = new CreateCharCommand()
        {
            Name = charName,
        };

        _webNetworkService.SendClientWebCommand(createCommand, _token);

    }
}

