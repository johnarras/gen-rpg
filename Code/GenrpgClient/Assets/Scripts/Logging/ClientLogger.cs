
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Setup.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ClientLogger : ILogService
{

    private UnityGameState _gs;
    public async Task Initialize(GameState gs, CancellationToken token)
    {
        _gs = gs as UnityGameState;
        await Task.CompletedTask;
    }

    private ClientConfig _config = null;
    private IDispatcher _dispatcher;
    public ClientLogger(ClientConfig config)
    {
        _config = config;
    }

    public async Task PrioritySetup(GameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public int SetupPriorityAscending() { return SetupPriorities.Logging; }


    public void Debug(string txt)
    {
        UnityEngine.Debug.Log(txt);
    }

    public void Error(string txt)
    {
        _dispatcher.Dispatch(_gs, new ShowFloatingText(txt, EFloatingTextArt.Error));
        UnityEngine.Debug.LogError(txt);
    }

    public void Exception(Exception e, string txt)
    {
        _dispatcher.Dispatch(_gs, new ShowFloatingText(txt + " " + e.Message + " " + e.StackTrace, EFloatingTextArt.Error));
        UnityEngine.Debug.LogError(txt + " " + e.Message + " " + e.StackTrace);
    }

    public void Info(string txt)
    {
        UnityEngine.Debug.Log(txt);
    }

    public void Message(string txt)
    {
        _dispatcher.Dispatch(_gs, new ShowFloatingText(txt, EFloatingTextArt.Message));
        UnityEngine.Debug.Log(txt);
    }

    public void Warning(string txt)
    {
        UnityEngine.Debug.LogWarning(txt);
    }
}
