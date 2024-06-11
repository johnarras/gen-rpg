
using System.Threading;
using UnityEngine;

public class AddOceans : BaseZoneGenerator
{
    public const float MaxSteepness = 25;
    public const float ChestChance = 0.1f;


    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
    }
}
