using System;

using UI.Screens.Constants;
using System.Threading;
using Genrpg.Shared.Website.Messages.Login;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using UnityEngine;
using System.Threading.Tasks;
using Genrpg.Shared.Website.Messages.Signup;
using Genrpg.Shared.Accounts.Constants;
using Genrpg.Shared.Utils;
using Assets.Scripts.UI.Screens;

public class SignupScreen : ErrorMessageScreen
{
    
    public GInputField NameInput;
    public GInputField ShareIdInput;
    public GInputField ReferrerIdInput;
    public GInputField EmailInput;   
    public GInputField PasswordInput1;
    public GInputField PasswordInput2;
    public GButton LoginButton;
    public GButton SignupButton;
    public GText ErrorText;

    protected IClientAuthService _authService;
    protected IRepositoryService _repoService;

    public override void ShowError(string errorMessage)
    {
        _uiService.SetText(ErrorText, errorMessage);
    }

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
        ShowError("");
        string email = EmailInput.Text;
        string name = NameInput.Text;
        string password1 = PasswordInput1.Text;
        string password2 = PasswordInput2.Text;
        string shareId = ShareIdInput.Text;
        string referrerId = ReferrerIdInput.Text;
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(name) && InitClient.EditorInstance.AccountSuffixId > 0)
        {
            long id = InitClient.EditorInstance.AccountSuffixId;
            name = "john" + id;
            email = name + "@gmail.com";
            shareId = name;
            referrerId = null;
            password1 = "password";
            password2 = "password";
            InitClient.EditorInstance.AccountSuffixId++;
        }
#endif
        if (name != null)
        {
            name = name.Trim();
        }

        if (string.IsNullOrEmpty(name) || 
            name.Length < AccountConstants.MinNameLength ||
            name.Length > AccountConstants.MaxNameLength)
        {
            _logService.Message($"Your Name must be between {AccountConstants.MinShareIdLength} and {AccountConstants.MaxShareIdLength} characters.");
            return;
        }

        if (String.IsNullOrEmpty(email) || email.IndexOf("@") < 0)
        {
            _logService.Message("Email must not be blank");
            return;
        }

        if (string.IsNullOrEmpty(shareId) || 
            shareId.Length < AccountConstants.MinShareIdLength ||
            shareId.Length > AccountConstants.MaxShareIdLength)
        { 
            _logService.Message($"Your ShareId must be between {AccountConstants.MinShareIdLength} and {AccountConstants.MaxShareIdLength} alphanumeric characters.");
            return;
        }

        bool allAlphanumeric = true;
        for (int s = 0; s < shareId.Length; s++)
        {
            if (!StrUtils.IsAlNum(shareId[s]))
            {
                allAlphanumeric = false;
                break;
            }
        }

        if (!allAlphanumeric)
        {
            _logService.Message($"Your ShareId must be between {AccountConstants.MinShareIdLength} and {AccountConstants.MaxShareIdLength} alphanumeric characters.");

            return;
        }

        if (password1 != password2)
        {
            _logService.Message("Passwords don't match");
            return;
        }

        if (string.IsNullOrEmpty(password1) || password1.Length < AccountConstants.MinPasswordLength)
        {
            _logService.Message($"Password must be at least {AccountConstants.MinPasswordLength} characters");
            return;
        }

        SignupCommand signupCommand = new SignupCommand()
        {
            Email = email,
            Password = password2,
            ShareId = shareId,
            ReferrerId = referrerId,
            Name = name,
            ClientVersion = AppUtils.Version,
            DeviceId = CryptoUtils.GetDeviceId(),
        };

        _authService.Signup(signupCommand, _token);

    }
}

