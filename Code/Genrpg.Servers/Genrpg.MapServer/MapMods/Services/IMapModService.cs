using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapMods.MapObjects;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MapMods.Services
{
    public interface IMapModService : IInitializable
    {
        void Process(IRandom rand, MapMod mapMod);
    }
}
