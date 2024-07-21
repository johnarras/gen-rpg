
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class MainMenuScreen : BaseScreen
{
    
    public GButton ExitMapButton;
    public GButton LogoutAccountButton;
    public GButton QuitGameButton;

    protected IClientAuthService _loginService;
    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
        _uIInitializable.SetButton(LogoutAccountButton, GetName(), ClickLogout);
        _uIInitializable.SetButton(QuitGameButton, GetName(), ClickQuit);
        _uIInitializable.SetButton(ExitMapButton, GetName(), ExitMap);


        await Task.CompletedTask;
    }


    private void ClickLogout()
    {
        _loginService.Logout();
    }

    private void ClickQuit()
    {
        AppUtils.Quit();
    }

    private void ExitMap()
    {
        _loginService.ExitMap();
    }
    

}

