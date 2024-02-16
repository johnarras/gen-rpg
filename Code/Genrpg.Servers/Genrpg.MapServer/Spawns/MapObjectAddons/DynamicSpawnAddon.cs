using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.MapObjectAddons
{
    public class DynamicSpawnAddon : BaseMapObjectAddon
    {
        public override long GetAddonType() { return MapObjectAddonTypes.DynamicSpawn; }

        public string ParentId { get; set; }
    }
}
