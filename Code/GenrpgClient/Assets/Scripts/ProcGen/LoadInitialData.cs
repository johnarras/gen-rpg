using System;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Players.Messages;
using GEntity = UnityEngine.GameObject;


public class LoadInitialData : BaseZoneGenerator
{
    protected IScreenService _screenService;
    private INetworkService _networkService;
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        TaskUtils.AddTask( LoadInitialMapData(gs),"loadinitialmapdata", token);
    }

    public async Task LoadInitialMapData(UnityGameState gs)
    {
        gs.md.HaveSetHeights = true;
        gs.md.HaveSetAlphaSplats = true;

        float delaySec = 1.0f;

        UnityAssetService.LoadSpeed = LoadSpeed.Fast;

        await Task.Delay(TimeSpan.FromSeconds(delaySec));

        while (_terrainManager.AddingPatches(gs))
        {
            await Task.Delay(TimeSpan.FromSeconds(delaySec));
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
                    await Task.Delay(1000);
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

        gs.logger.Debug("LOADINTOMAP START");
        await Task.CompletedTask;
    }



}
