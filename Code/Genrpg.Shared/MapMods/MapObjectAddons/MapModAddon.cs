using MessagePack;
using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapMods.MapObjectAddons
{
    [MessagePackObject]
    public class MapModAddon : BaseMapObjectAddon
    {
        public override long GetAddonType() { return MapObjectAddonTypes.MapMod; }

        [Key(0)] public List<MapModEffect> Effects { get; set; } = new List<MapModEffect>();

        [Key(1)] public long OwnerEntityTypeId { get; set; }
        [Key(2)] public string OwnerId { get; set; }
        [Key(3)] public float Radius { get; set; }
        [Key(4)] public int TriggerTimes { get; set; }
    }
}
