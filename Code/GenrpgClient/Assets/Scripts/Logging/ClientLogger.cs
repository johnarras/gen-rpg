
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Setup.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ClientLogger : ILogService
{
    public async Task Initialize(GameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }

    private ClientConfig _config = null;
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
        FloatingTextScreen.Instance?.ShowError(txt);
        UnityEngine.Debug.LogError(txt);
    }

    public void Exception(Exception e, string txt)
    {
        FloatingTextScreen.Instance?.ShowError(txt);
        UnityEngine.Debug.LogError(txt + " " + e.Message + " " + e.StackTrace);
    }

    public void Info(string txt)
    {
        UnityEngine.Debug.Log(txt);
    }

    public void Message(string txt)
    {
        FloatingTextScreen.Instance?.ShowMessage(txt);
        UnityEngine.Debug.Log(txt);
    }

    public void Warning(string txt)
    {
        UnityEngine.Debug.LogWarning(txt);
    }
}
