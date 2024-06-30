using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Zones.Settings
{
    [MessagePackObject]
    public class ZoneUnitSpawn
    {
        [Key(0)] public long UnitTypeId { get; set; }
        [Key(1)] public double Chance { get; set; }
    }
}
