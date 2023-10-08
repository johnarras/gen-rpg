
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using GEntity = UnityEngine.GameObject;


using Genrpg.Shared.Core.Entities;



using System.Threading.Tasks;
using System.Threading;

public class SetBelowLandTerrainTextures : BaseZoneGenerator
{
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
    }
}
        