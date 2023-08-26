
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : BaseScreen
{
    [SerializeField]
    private Button _exitMapButton;

    [SerializeField]
    private Button _logoutAccountButton;

    [SerializeField]
    private Button _quitGameButton;


    protected IClientLoginService _loginService;
    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        UIHelper.SetButton(_logoutAccountButton, GetAnalyticsName(), ClickLogout);
        UIHelper.SetButton(_quitGameButton, GetAnalyticsName(), ClickQuit);
        UIHelper.SetButton(_exitMapButton, GetAnalyticsName(), ExitMap);

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

