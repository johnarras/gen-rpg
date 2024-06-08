using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.Units.Entities;
using GEntity = UnityEngine.GameObject;


public class LoadInitialData : BaseZoneGenerator
{
    protected IScreenService _screenService;
    private IRealtimeNetworkService _networkService;
    private IPlayerManager _playerManager;
    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);
        LoadInitialMapData(token).Forget();
    }

    public async UniTask LoadInitialMapData(CancellationToken token)
    {
        _md.HaveSetHeights = true;
        _md.HaveSetAlphaSplats = true;

        float delaySec = 1.0f;

        _terrainManager.SetFastLoading();

        await UniTask.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken: token);

        while (_terrainManager.AddingPatches())
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken: token);
        }
        if (!_playerManager.TryGetUnit(out Unit unit))
        {
            return;
        }

        GEntity go = _playerManager.GetEntity();
        if (unit != null && go != null)
        {
            float height = 0;

            do
            {
                height = _terrainManager.SampleHeight(unit.X, unit.Z);
                go.transform().position = GVector3.Create(unit.X, height, unit.Z);
                go.transform().eulerAngles = GVector3.Create(0, unit.Rot, 0);
                if (height == 0)
                {
                    await UniTask.Delay(1000, cancellationToken: token);
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

        _logService.Debug("LOADINTOMAP START " + _gs.user.SessionId);
        await UniTask.CompletedTask;
    }



}
