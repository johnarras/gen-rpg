
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Website.Messages.CreateChar;
using UnityEngine;
using System.Threading.Tasks;

public class CharacterCreateScreen : BaseScreen
{
    private IClientWebService _webNetworkService;
    
    public GInputField NameInput;
    public GButton CreateButton;
    public GButton BackButton;

    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
        _uIInitializable.SetButton(CreateButton, GetName(), ClickCreate);
        _uIInitializable.SetButton(BackButton, GetName(),ClickBack);

        await Task.CompletedTask;
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

