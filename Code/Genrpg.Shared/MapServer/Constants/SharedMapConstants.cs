using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapServer.Constants
{
    [MessagePackObject]
    public class SharedMapConstants
    {
        public const int TerrainPatchSize = 129;
        public const int MapObjectGridSize = 32;
        public const int DefaultHeightmapSize = (TerrainPatchSize - 1) * 4 + 1;
        public const int MinBaseZoneId = 8;
        public const int MaxBaseZoneId = 12;
        public const int MapZoneStartId = MaxBaseZoneId + 1;
        public const int MapSpawnArraySize = 100;
    }
}
