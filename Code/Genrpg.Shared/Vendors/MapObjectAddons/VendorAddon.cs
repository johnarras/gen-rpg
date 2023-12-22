using MessagePack;
using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.Vendors.WorldData;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.DataStores.Interfaces;

namespace Genrpg.Shared.Vendors.MapObjectAddons
{
    [MessagePackObject]
    public class VendorAddon : BaseMapObjectAddon
    {
        public override long GetAddonType() { return MapObjectAddonTypes.Vendor; }

        [Key(0)] public int ItemCount { get; set; }
        [Key(1)] public long NPCTypeId { get; set; }
        [Key(2)] public DateTime LastRefreshTime { get; set; } = DateTime.MinValue;
        [Key(3)] public List<VendorItem> Items { get; set; } = new List<VendorItem>();

    }
}
