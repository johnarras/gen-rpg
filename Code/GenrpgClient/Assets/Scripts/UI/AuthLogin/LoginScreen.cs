
using System.Threading.Tasks;
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Login.Messages.Login;

public class LoginScreen : BaseScreen
{
    
    public GInputField EmailInput;
    public GInputField PasswordInput;
    public GButton LoginButton;
    public GButton SignupButton;

    protected IClientLoginService _loginService;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        UIHelper.SetButton(LoginButton, GetAnalyticsName(), ClickLogin);
        UIHelper.SetButton(SignupButton, GetAnalyticsName(), ClickSignup);
        await Task.CompletedTask;
    }

    public void ClickSignup()
    {
        if (!CanClick("signup"))
        {
            return;
        }

        _screenService.Open(_gs, ScreenId.Signup);
        _screenService.Close(_gs, ScreenId.Login);
    }

    public void ClickLogin()
    {
        if (!CanClick("login"))
        {
            return;
        }

        if (string.IsNullOrEmpty(EmailInput.Text))
        {
            _gs.logger.Error("Missing email");
            return;
        }
        if (string.IsNullOrEmpty(PasswordInput.Text))
        {
            _gs.logger.Error("Missing password");
            return;
        }

        LoginCommand loginCommand = new LoginCommand()
        {
            Email = EmailInput.Text,
            Password = PasswordInput.Text,
        };

        TaskUtils.AddTask(_loginService.LoginToServer(_gs, loginCommand, _token));
    }
}

