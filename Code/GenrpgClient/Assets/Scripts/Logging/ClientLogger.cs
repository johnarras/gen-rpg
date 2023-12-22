
using Genrpg.Shared.Logs.Interfaces;
using System;

public class ClientLogger : ILogSystem
{

    private UnityGameState _gs;
    public ClientLogger(UnityGameState gs)
    {
        _gs = gs;
    }

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
