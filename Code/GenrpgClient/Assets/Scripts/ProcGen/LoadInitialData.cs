using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Players.Messages;
using UI.Screens.Constants;
using UnityEngine;


public class LoadInitialData : BaseZoneGenerator
{
    protected IScreenService _screenService;
    private INetworkService _networkService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        LoadInitialMapData(gs).Forget();
    }

    public async UniTask LoadInitialMapData(UnityGameState gs)
    {
        gs.md.HaveSetHeights = true;
        gs.md.HaveSetAlphaSplats = true;

        float delaySec = 1.0f;

        UnityAssetService.LoadSpeed = LoadSpeed.Fast;

        await UniTask.Delay(TimeSpan.FromSeconds(delaySec));

        while (_terrainManager.AddingPatches(gs))
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySec));
        }
        RenderSettings.fog = true;
        RenderSettings.ambientIntensity = 1.0f;

        UnityAssetService.LoadSpeed = LoadSpeed.Normal;

        Character ch = PlayerObject.GetUnit() as Character;
        GameObject go = PlayerObject.Get();
        if (ch != null && go != null)
        {
            float height = gs.md.SampleHeight(gs, ch.X, MapConstants.MapHeight * 2, ch.Z);
            go.transform.position = new Vector3(ch.X, height, ch.Z);
            go.transform.eulerAngles = new Vector3(0, ch.Rot, 0);
        }
        _networkService.SendMapMessage(new AddPlayer()
        {
            UserId = gs.user.Id,
            SessionId = gs.user.SessionId,
            CharacterId = gs.ch.Id,
        });

        gs.logger.Debug("LOADINTOMAP START");
        await UniTask.CompletedTask;
    }



}
