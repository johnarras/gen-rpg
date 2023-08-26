using System;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Login.Constants;
using UI.Screens.Constants;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.Login.Messages.Login;

public class SignupScreen : BaseScreen
{
    [SerializeField]
    private InputField _nameInput;
    [SerializeField]
    private InputField _emailInput;
    [SerializeField]
    private InputField _passwordInput1;
    [SerializeField]
    private InputField _passwordInput2;
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

        string name = UIHelper.GetInputText(_nameInput);
        string password1 = UIHelper.GetInputText(_passwordInput1);
        string password2 = UIHelper.GetInputText(_passwordInput2);
        string email = UIHelper.GetInputText(_emailInput);

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

        _loginService.LoginToServer(_gs, loginCommand, _token).Forget();

    }
}

