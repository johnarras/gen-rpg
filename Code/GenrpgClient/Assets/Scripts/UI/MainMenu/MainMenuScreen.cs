
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class MainMenuScreen : BaseScreen
{
    
    public GButton ExitMapButton;
    public GButton LogoutAccountButton;
    public GButton QuitGameButton;

    protected IClientAuthService _loginService;
    private IClientAppService _clientAppService;
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        _uiService.SetButton(LogoutAccountButton, GetName(), ClickLogout);
        _uiService.SetButton(QuitGameButton, GetName(), ClickQuit);
        _uiService.SetButton(ExitMapButton, GetName(), ExitMap);


        await Task.CompletedTask;
    }


    private void ClickLogout()
    {
        _loginService.Logout();
    }

    private void ClickQuit()
    {
        _clientAppService.Quit();
    }

    private void ExitMap()
    {
        _loginService.ExitMap();
    }
    

}

