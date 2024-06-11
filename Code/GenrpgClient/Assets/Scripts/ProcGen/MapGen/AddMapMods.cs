
using System.Threading;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Entities.Constants;
using UnityEngine;

public class AddMapMods : BaseZoneGenerator
{
    const int skip = MapConstants.TerrainPatchSize / 2;
    const int start = skip * 3 / 2;
    public override async Awaitable Generate(CancellationToken token)
    {
        
        for (int x = start; x < _mapProvider.GetMap().GetHwid()-start; x+= skip)
        {
            for (int z = start; z < _mapProvider.GetMap().GetHhgt()-start; z += skip)
            {
                if (_zoneGenService.FindMapLocation(x,z,2) != null)
                {
                    continue;
                }

                InitSpawnData initData = new InitSpawnData()
                {
                    EntityTypeId = EntityTypes.MapMod,
                    EntityId = 0,
                    SpawnX = x,
                    SpawnZ = z,
                };

                _mapProvider.GetSpawns().AddSpawn(initData);
            }
        }
    }
}