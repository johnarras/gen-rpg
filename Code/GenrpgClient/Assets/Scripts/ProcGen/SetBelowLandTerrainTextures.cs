
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;


using Genrpg.Shared.Core.Entities;


using Services;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SetBelowLandTerrainTextures : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
    }
}
        