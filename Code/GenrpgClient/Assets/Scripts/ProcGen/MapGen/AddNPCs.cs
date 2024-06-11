using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using Genrpg.Shared.Core.Entities;



using GEntity = UnityEngine.GameObject;

using Genrpg.Shared.Interfaces;

using System.Threading;
using UnityEngine;

public class AddNPCs : BaseZoneGenerator
{
    protected IMapGenService _mapGenService;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        _mapGenService.AddNPCs(_gs);
    }
}