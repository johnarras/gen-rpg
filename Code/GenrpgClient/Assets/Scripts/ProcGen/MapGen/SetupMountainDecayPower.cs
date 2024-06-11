
using Genrpg.Shared.Utils;
using System.Threading;
using UnityEngine;

public class SetupMountainDecayPower : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 100000000 + 3452329);
        float powerpers = MathUtils.FloatRange(0.2f, 0.4f, rand);
        int poweroctaves = 2;
        float poweramp = MathUtils.FloatRange(0.3f, 0.5f, rand);
        float powerFreq = MathUtils.FloatRange(0.04f, 0.14f, rand) * _mapProvider.GetMap().GetHwid();
        
        _md.mountainDecayPower = _noiseService.Generate(powerpers, powerFreq, poweramp, poweroctaves, rand.Next(), _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());
        
    }
}