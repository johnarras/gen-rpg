using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapObjects.MapObjectAddons.Entities
{
    public abstract class BaseMapObjectAddon : IMapObjectAddon
    {
        public abstract long GetAddonType();
    }
}
