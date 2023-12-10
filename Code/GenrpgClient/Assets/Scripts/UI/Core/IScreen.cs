
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using System.Threading;
using UI.Screens.Constants;

public interface IScreen
{
    ScreenId ScreenId { get; }
    string Name { get; set; }
    UniTask StartOpen(object data, CancellationToken token);
    void StartClose();
    void ErrorClose(string txt);
    void OnInfoChanged();
    bool BlockMouse();
    string GetAnalyticsName();
}