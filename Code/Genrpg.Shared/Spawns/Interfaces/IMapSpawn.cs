using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spawns.Interfaces
{
    public interface IMapSpawn
    {
        string ObjId { get; set; }
        DateTime LastSpawnTime { get; set; }
        string Name { get; set; }
        long EntityTypeId { get; set; }
        long EntityId { get; set; }
        float X { get; set; }
        float Z { get; set; }
        float Rot { get; set; }
        long ZoneId { get; set; }
        int SpawnSeconds { get; set; }
        long FactionTypeId { get; set; }
        int OverrideZonePercent { get; set; }
        List<IMapObjectAddon> GetAddons();
        long GetAddonBits();
    }
}
