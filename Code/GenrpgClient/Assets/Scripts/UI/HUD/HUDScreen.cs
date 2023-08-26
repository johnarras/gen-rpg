
using Assets.Scripts.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using UI;
using UnityEngine;

public class HUDScreen : BaseScreen
{
    [SerializeField]
    private ZoneUI _zoneUI;

    [SerializeField]
    private MinimapUI _minimap;

    [SerializeField]
    private UnitFrameContainer _unitFrame;

    [SerializeField]
    private NetworkStatusUI _networkStatus;

    [SerializeField]
    private ActionBars _actionBars;

    [SerializeField]
    private ChatWindow _chatWindow;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        _zoneUI?.Init(token);
        _minimap?.Init(token);
        _unitFrame?.Init(token);
        _networkStatus?.Init(token);
        _actionBars?.Init(token);
        await UniTask.CompletedTask;
    }
}
