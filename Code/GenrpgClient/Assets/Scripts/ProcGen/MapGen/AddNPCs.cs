using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using Genrpg.Shared.Core.Entities;



using GEntity = UnityEngine.GameObject;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;

using System.Threading;

public class AddNPCs : BaseZoneGenerator
{
    protected IMapGenService _mapGenService;
    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);
        _mapGenService.AddNPCs(_gs);
    }
}