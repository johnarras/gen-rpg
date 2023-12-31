using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Players.Messages;
using GEntity = UnityEngine.GameObject;


public class LoadInitialData : BaseZoneGenerator
{
    protected IScreenService _screenService;
    private IRealtimeNetworkService _networkService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        LoadInitialMapData(gs, token).Forget();
    }

    public async UniTask LoadInitialMapData(UnityGameState gs, CancellationToken token)
    {
        gs.md.HaveSetHeights = true;
        gs.md.HaveSetAlphaSplats = true;

        float delaySec = 1.0f;

        UnityAssetService.LoadSpeed = LoadSpeed.Fast;

        await UniTask.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken: token);

        while (_terrainManager.AddingPatches(gs))
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken: token);
        }
        UnityAssetService.LoadSpeed = LoadSpeed.Normal;

        Character ch = PlayerObject.GetUnit() as Character;
        GEntity go = PlayerObject.Get();
        if (ch != null && go != null)
        {
            float height = 0;

            do
            {
                height = gs.md.SampleHeight(gs, ch.X, MapConstants.MapHeight * 2, ch.Z);
                go.transform().position = GVector3.Create(ch.X, height, ch.Z);
                go.transform().eulerAngles = GVector3.Create(0, ch.Rot, 0);
                if (height == 0)
                {
                    await UniTask.Delay(1000, cancellationToken: token);
                }
            }
            while (height == 0);
        }
        _networkService.SendMapMessage(new AddPlayer()
        {
            UserId = gs.user.Id,
            SessionId = gs.user.SessionId,
            CharacterId = gs.ch.Id,
        });

        gs.logger.Debug("LOADINTOMAP START " + gs.user.SessionId);
        await UniTask.CompletedTask;
    }



}
