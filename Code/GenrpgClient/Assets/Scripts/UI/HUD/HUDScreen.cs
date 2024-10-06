
using Assets.Scripts.UI;

using System.Threading;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public class HUDScreen : BaseScreen
{
    
    public ZoneUI _zoneUI;

    
    public MinimapUI _minimap;

    
    public UnitFrameContainer _unitFrame;

    
    public NetworkStatusUI _networkStatus;

    
    public ActionBars _actionBars;

    
    public ChatWindow _chatWindow;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        _zoneUI?.Init(token);
        _minimap?.Init(token);
        _unitFrame?.Init(token);
        _networkStatus?.Init(token);
        _actionBars?.Init(token);

        await Task.CompletedTask;
    }
}
