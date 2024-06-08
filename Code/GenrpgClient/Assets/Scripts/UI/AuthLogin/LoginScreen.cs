
using Cysharp.Threading.Tasks;
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;

public class LoginScreen : BaseScreen
{
    
    public GInputField EmailInput;
    public GInputField PasswordInput;
    public GButton LoginButton;
    public GButton SignupButton;

    protected IClientLoginService _loginService;
    protected IRepositoryService _repoService;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        _uIInitializable.SetButton(LoginButton, GetName(), ClickLogin);
        _uIInitializable.SetButton(SignupButton, GetName(), ClickSignup);
        await UniTask.CompletedTask;
    }

    public void ClickSignup()
    {
        _screenService.Open(ScreenId.Signup);
        _screenService.Close(ScreenId.Login);
    }

    public void ClickLogin()
    {
        if (string.IsNullOrEmpty(EmailInput.Text))
        {
            _logService.Error("Missing email");
            return;
        }
        if (string.IsNullOrEmpty(PasswordInput.Text))
        {
            _logService.Error("Missing password");
            return;
        }

        LoginCommand loginCommand = new LoginCommand()
        {
            Email = EmailInput.Text,
            Password = PasswordInput.Text,
        };

        _loginService.LoginToServer(loginCommand, _token).Forget();
    }
}

