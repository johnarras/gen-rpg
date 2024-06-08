using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spawns.WorldData;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Genrpg.Shared.MapServer.Services
{
    public class MapProvider : IMapProvider
    {
        private Map _map;
        private MapSpawnData _spawns;
        private bool[,] _pathfinding;

        public void SetMap(Map map)
        {
            _map = map;
        }

        public Map GetMap()
        {
            return _map;
        }

        public void SetSpawns(MapSpawnData spawns)
        {
            _spawns = spawns;
        }

        public MapSpawnData GetSpawns()
        {
            return _spawns;
        }

        public void SetPathfinding(bool[,] pathfinding)
        {
            _pathfinding = pathfinding;
        }

        public bool[,] GetPathfinding()
        {
            return _pathfinding;
        }
    }
}
