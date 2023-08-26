
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UI.Screens.Constants;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.Login.Messages.Login;

public class LoginScreen : BaseScreen
{
    [SerializeField]
    private InputField _emailInput;
    [SerializeField]
    private InputField _passwordInput;
    [SerializeField]
    private Button _loginButton;
    [SerializeField]
    private Button _signupButton;

    protected IClientLoginService _loginService;
    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        UIHelper.SetButton(_loginButton, GetAnalyticsName(), ClickLogin);
        UIHelper.SetButton(_signupButton, GetAnalyticsName(), ClickSignup);
        await UniTask.CompletedTask;
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

        if (_emailInput == null || string.IsNullOrEmpty(_emailInput.text))
        {
            _gs.logger.Error("Missing email");
            return;
        }
        if (_passwordInput == null || string.IsNullOrEmpty(_passwordInput.text))
        {
            _gs.logger.Error("Missing password");
            return;
        }

        LoginCommand loginCommand = new LoginCommand()
        {
            Email = _emailInput.text,
            Password = _passwordInput.text,
        };

        _loginService.LoginToServer(_gs, loginCommand, _token).Forget();
    }
}

