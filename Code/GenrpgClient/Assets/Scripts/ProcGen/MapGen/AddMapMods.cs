using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using Genrpg.Shared.Core.Entities;



using GEntity = UnityEngine.GameObject;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;

using System.Threading;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Entities.Constants;

public class AddMapMods : BaseZoneGenerator
{
    const int skip = MapConstants.TerrainPatchSize / 2;
    const int start = skip * 3 / 2;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await UniTask.CompletedTask;
        for (int x = start; x < gs.map.GetHwid()-start; x+= skip)
        {
            for (int z = start; z < gs.map.GetHhgt()-start; z += skip)
            {
                if (_zoneGenService.FindMapLocation(gs,x,z,2) != null)
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

                gs.spawns.AddSpawn(initData);
            }
        }
    }
}