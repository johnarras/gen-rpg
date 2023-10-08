using System;
using System.Threading.Tasks;
using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Login.Messages.Login;

public class SignupScreen : BaseScreen
{
    
    public GInputField NameInput;
    public GInputField EmailInput;
    public GInputField PasswordInput1;
    public GInputField PasswordInput2;
    public GButton LoginButton;
    public GButton SignupButton;

    protected IClientLoginService _loginService;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        UIHelper.SetButton(LoginButton, GetAnalyticsName(), ClickLogin);
        UIHelper.SetButton(SignupButton, GetAnalyticsName(), ClickSignup);
        await Task.CompletedTask;
    }

    public void ClickLogin()
    {
        if (!CanClick("login"))
        {
            return;
        }

        _screenService.Open(_gs, ScreenId.Login);
        _screenService.Close(_gs, ScreenId.Signup);
    }


    public void ClickSignup()
    {
        if (!CanClick("signup"))
        {
            return;
        }
        string email = EmailInput.Text;
        string name = NameInput.Text;
        string password1 = PasswordInput1.Text;
        string password2 = PasswordInput2.Text;

        if (string.IsNullOrEmpty(name))
        {
            _gs.logger.Message("Name must not be blank");
            return;
        }

        if (String.IsNullOrEmpty(email))
        {
            _gs.logger.Message("Email must not be blank");
            return;
        }

        if (password1 != password2)
        {
            _gs.logger.Message("Passwords don't match");
            return;
        }

        if (string.IsNullOrEmpty(password1))
        {
            _gs.logger.Message("Password isn't strong enough");
            return;
        }

        LoginCommand loginCommand = new LoginCommand()
        {
            Email = email,
            Password = password2,
            Name = name,
        };

        TaskUtils.AddTask(_loginService.LoginToServer(_gs, loginCommand, _token));

    }
}

