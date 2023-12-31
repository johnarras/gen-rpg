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
        UIHelper.SetButton(CreateButton, GetAnalyticsName(), ClickCreate);
        UIHelper.SetButton(BackButton, GetAnalyticsName(),ClickBack);
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

        string charName = NameInput.Text;
        if (string.IsNullOrEmpty(charName))
        {
            _gs.logger.Message("You need to choose a name!");
            return;
        }

        CreateCharCommand createCommand = new CreateCharCommand()
        {
            Name = charName,
        };

        _webNetworkService.SendClientWebCommand(createCommand, _token);

    }
}

