using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapMods.MapObjectAddons;
using Genrpg.Shared.MapMods.MapObjects;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.MapMods.Helpers
{
    public interface IMapModEffectHelper : ISetupDictionaryItem<long>
    {
        void Process(IRandom rand, MapMod mapMod, MapModAddon addon, MapModEffect effect);
    }
}
