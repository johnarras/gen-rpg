using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spawns.WorldData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapServer.Services
{
    public interface IMapProvider : IInjectable
    {
        Map GetMap();
        void SetMap(Map map);

        MapSpawnData GetSpawns();
        void SetSpawns(MapSpawnData mapSpawnData);

    }
}
