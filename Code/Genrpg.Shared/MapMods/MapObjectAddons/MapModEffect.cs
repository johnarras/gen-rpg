using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapMods.MapObjectAddons
{
    [MessagePackObject]
    public class MapModEffect
    {
        [Key(0)] public long MapModEffectTypeId { get; set; }
        [Key(1)] public long Radius { get; set; }
        [Key(2)] public long EntityId { get; set; }
        [Key(3)] public long MaxQuantity { get; set; }
        [Key(4)] public DateTime EndTime { get; set; } = DateTime.MinValue;
        [Key(5)] public long CurrQuantity { get; set; }
        [Key(6)] public string Name { get; set; }
    }
}
