using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using Genrpg.Shared.Core.Entities;




using GEntity = UnityEngine.GameObject;
using Cysharp.Threading.Tasks;
using System.Threading;

public class AddOceans : BaseZoneGenerator
{
    public const float MaxSteepness = 25;
    public const float ChestChance = 0.1f;


    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
    }
}
