using System;

using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using UnityEngine;
using System.Threading.Tasks;

public class SignupScreen : BaseScreen
{
    
    public GInputField NameInput;
    public GInputField EmailInput;
    public GInputField PasswordInput1;
    public GInputField PasswordInput2;
    public GButton LoginButton;
    public GButton SignupButton;

    protected IClientLoginService _loginService;
    protected IRepositoryService _repoService;

    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
        _uIInitializable.SetButton(LoginButton, GetName(), ClickLogin);
        _uIInitializable.SetButton(SignupButton, GetName(), ClickSignup);
        await Task.CompletedTask;
    }

    public void ClickLogin()
    {
        _screenService.Open(ScreenId.Login);
        _screenService.Close(ScreenId.Signup);
    }


    public void ClickSignup()
    {
        string email = EmailInput.Text;
        string name = NameInput.Text;
        string password1 = PasswordInput1.Text;
        string password2 = PasswordInput2.Text;

        if (string.IsNullOrEmpty(name))
        {
            _logService.Message("Name must not be blank");
            return;
        }

        if (String.IsNullOrEmpty(email))
        {
            _logService.Message("Email must not be blank");
            return;
        }

        if (password1 != password2)
        {
            _logService.Message("Passwords don't match");
            return;
        }

        if (string.IsNullOrEmpty(password1))
        {
            _logService.Message("Password isn't strong enough");
            return;
        }

        LoginCommand loginCommand = new LoginCommand()
        {
            Email = email,
            Password = password2,
            Name = name,
        };

        _loginService.LoginToServer(loginCommand, _token);

    }
}

