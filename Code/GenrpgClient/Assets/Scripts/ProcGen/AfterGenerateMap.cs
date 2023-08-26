
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;


using Genrpg.Shared.Core.Entities;


using Services;
using Cysharp.Threading.Tasks;
using Entities;
using UI.Screens.Constants;
using System.Threading;

public class AfterGenerateMap : BaseZoneGenerator
{

    protected IScreenService _screenService;
    public override async UniTask Generate (UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
        {
            _screenService.CloseAll(gs);
            _screenService.Open(gs, ScreenId.CharacterSelect);
        }
        await UniTask.CompletedTask;
	}
	

}
	
