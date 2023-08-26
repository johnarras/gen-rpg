using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using Genrpg.Shared.Core.Entities;


using Services;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Services.ProcGen;
using System.Threading;

public class AddNPCs : BaseZoneGenerator
{
    protected IMapGenService _mapGenService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        _mapGenService.AddNPCs(gs);
    }
}