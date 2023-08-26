using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using Services.ProcGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class SetupMountainDecayPower : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        MyRandom rand = new MyRandom(gs.map.Seed % 100000000 + 3452329);
        float powerpers = MathUtils.FloatRange(0.2f, 0.4f, rand);
        int poweroctaves = 2;
        float poweramp = MathUtils.FloatRange(0.3f, 0.5f, rand);
        float powerFreq = MathUtils.FloatRange(0.04f, 0.14f, rand) * gs.map.GetHwid();
        
        gs.md.mountainDecayPower = _noiseService.Generate(gs, powerpers, powerFreq, poweramp, poweroctaves, rand.Next(), gs.map.GetHwid(), gs.map.GetHhgt());
        
    }
}