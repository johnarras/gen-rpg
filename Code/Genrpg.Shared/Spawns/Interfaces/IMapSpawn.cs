using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spawns.Interfaces
{
    public interface IMapSpawn
    {
        string MapObjectId { get; set; }
        DateTime LastSpawnTime { get; set; }
        long EntityTypeId { get; set; }
        long EntityId { get; set; }
        float X { get; set; }
        float Z { get; set; }
        long ZoneId { get; set; }
        int SpawnSeconds { get; set; }
        long FactionTypeId { get; set; }
        int OverrideZonePercent { get; set; }
        string GetName();
    }
}
