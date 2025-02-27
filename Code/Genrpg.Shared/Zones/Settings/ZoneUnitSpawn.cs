using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Zones.Settings
{
    [MessagePackObject]
    public class ZoneUnitSpawn : IWeightedItem
    {
        [Key(0)] public long UnitTypeId { get; set; }
        [Key(1)] public double Weight { get; set; }
    }
}
