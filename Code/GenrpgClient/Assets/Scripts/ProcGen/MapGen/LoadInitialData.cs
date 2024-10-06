using System.Threading;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Units.Entities;
using UnityEngine;


public class LoadInitialData : BaseZoneGenerator
{
    protected IScreenService _screenService;
    private IRealtimeNetworkService _networkService;
    private IPlayerManager _playerManager;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        TaskUtils.ForgetAwaitable(LoadInitialMapData(token));
    }

    public async Awaitable LoadInitialMapData(CancellationToken token)
    {
        _md.HaveSetHeights = true;
        _md.HaveSetAlphaSplats = true;

        float delaySec = 1.0f;

        _terrainManager.SetFastLoading();

        await Awaitable.WaitForSecondsAsync(delaySec, cancellationToken: token);

        while (_terrainManager.AddingPatches())
        {
            await Awaitable.WaitForSecondsAsync(delaySec, cancellationToken: token);
        }

        Unit unit = null;
        while (!_playerManager.TryGetUnit(out unit))
        {
            await Awaitable.WaitForSecondsAsync(delaySec, cancellationToken: token);
        }

        GameObject go = _playerManager.GetPlayerGameObject();
        if (unit != null && go != null)
        {
            float height = 0;

            do
            {
                height = _terrainManager.SampleHeight(unit.X, unit.Z);
                go.transform.position = new Vector3(unit.X, height, unit.Z);
                go.transform.eulerAngles = new Vector3(0, unit.Rot, 0);
                if (height == 0)
                {
                    await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);
                }
            }
            while (height == 0);
        }
        _networkService.SendMapMessage(new AddPlayer()
        {
            UserId = _gs.user.Id,
            SessionId = _gs.user.SessionId,
            CharacterId = _gs.ch.Id,
        });

        PlayerController controller = go.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.StartUpdates();
        }

        _logService.Debug("LOADINTOMAP START " + _gs.user.SessionId);
        
    }



}
