

using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Website.Messages.Login;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using UnityEngine;
using System.Threading.Tasks;
using Assets.Scripts.UI.Screens;
using System;

public class LoginScreen : ErrorMessageScreen
{
    
    public GInputField EmailInput;
    public GInputField PasswordInput;
    public GButton LoginButton;
    public GButton SignupButton;
    public GText ErrorText;

    protected IClientAuthService _loginService;
    protected IRepositoryService _repoService;

    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
        _uIInitializable.SetButton(LoginButton, GetName(), ClickLogin);
        _uIInitializable.SetButton(SignupButton, GetName(), ClickSignup);

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

        LoginCommand loginCommand = new LoginCommand()
        {
            Email = EmailInput.Text,
            Password = PasswordInput.Text,
            ClientVersion = AppUtils.Version,
            DeviceId = CryptoUtils.GetDeviceId(),
        };

        _loginService.LoginToServer(loginCommand, _token);
    }
}

