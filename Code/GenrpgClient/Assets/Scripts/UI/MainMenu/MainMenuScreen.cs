
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
        UIHelper.SetButton(LogoutAccountButton, GetAnalyticsName(), ClickLogout);
        UIHelper.SetButton(QuitGameButton, GetAnalyticsName(), ClickQuit);
        UIHelper.SetButton(ExitMapButton, GetAnalyticsName(), ExitMap);

        await UniTask.CompletedTask;
    }


    private void ClickLogout()
    {
        if (!CanClick("logout"))
        {
            return;
        }

        _loginService.Logout(_gs);
    }

    private void ClickQuit()
    {
        if (!CanClick("quit"))
        {
            return;
        }

        AppUtils.Quit();
    }

    private void ExitMap()
    {
        if (!CanClick("exit"))
        {
            return;
        }
        _loginService.ExitMap(_gs);
    }
    

}

