
using System.Threading;
using Cysharp.Threading.Tasks;

public class MainMenuScreen : BaseScreen
{
    
    public GButton ExitMapButton;
    public GButton LogoutAccountButton;
    public GButton QuitGameButton;

    protected IClientLoginService _loginService;
    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        _uIInitializable.SetButton(LogoutAccountButton, GetName(), ClickLogout);
        _uIInitializable.SetButton(QuitGameButton, GetName(), ClickQuit);
        _uIInitializable.SetButton(ExitMapButton, GetName(), ExitMap);

        await UniTask.CompletedTask;
    }


    private void ClickLogout()
    {
        _loginService.Logout(_gs);
    }

    private void ClickQuit()
    {
        AppUtils.Quit();
    }

    private void ExitMap()
    {
        _loginService.ExitMap(_gs);
    }
    

}

