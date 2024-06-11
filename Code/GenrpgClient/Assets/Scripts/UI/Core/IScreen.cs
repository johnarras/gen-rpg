

using Genrpg.Shared.Core.Entities;
using System.Threading;
using UI.Screens.Constants;
using UnityEngine;

public interface IScreen
{
    ScreenId ScreenID { get; }
    Awaitable StartOpen(object data, CancellationToken token);
    void StartClose();
    void ErrorClose(string txt);
    void OnInfoChanged();
    bool BlockMouse();
    string GetName();
}