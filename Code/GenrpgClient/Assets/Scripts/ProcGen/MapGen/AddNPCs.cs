
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