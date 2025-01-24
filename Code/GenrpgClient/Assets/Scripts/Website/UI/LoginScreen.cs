
using System.Threading;
using Genrpg.Shared.DataStores.Entities;
using System.Threading.Tasks;
using Assets.Scripts.UI.Screens;
using Genrpg.Shared.UI.Entities;
using Genrpg.Shared.Accounts.WebApi.Login;

public class LoginScreen : ErrorMessageScreen
{
    
    public GInputField EmailInput;
    public GInputField PasswordInput;
    public GButton LoginButton;
    public GButton SignupButton;
    public GText ErrorText;

    protected IClientAuthService _loginService;
    protected IRepositoryService _repoService;
    protected IClientAppService _clientAppService;
    protected IClientCryptoService _clientCryptoService;    
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        _uiService.SetButton(LoginButton, GetName(), ClickLogin);
        _uiService.SetButton(SignupButton, GetName(), ClickSignup);

        await Task.CompletedTask;
    }

    public override void ShowError(string errorMessage)
    {
        _uiService.SetText(ErrorText, errorMessage);
    }

    public void ClickSignup()
    {
        _screenService.Open(ScreenId.Signup);
        _screenService.Close(ScreenId.Login);
    }

    public void ClickLogin()
    {
        ShowError("");
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

        LoginRequest loginCommand = new LoginRequest()
        {
            Email = EmailInput.Text,
            Password = PasswordInput.Text,
            ClientVersion = _clientAppService.Version,
            DeviceId = _clientCryptoService.GetDeviceId(),
        };

        _loginService.LoginToServer(loginCommand, _token);
    }
}

