

using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.WorldData;
using System.Threading;
using UnityEngine;

public class GenerateMap : BaseZoneGenerator
{
    protected IMapGenService _mapGenService;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        _mapProvider.SetSpawns(new MapSpawnData() { Id = _mapProvider.GetMap().Id.ToString() });
        _mapProvider.SetMap(_mapGenService.GenerateMap(_mapProvider.GetMap()));
        
    }
}
