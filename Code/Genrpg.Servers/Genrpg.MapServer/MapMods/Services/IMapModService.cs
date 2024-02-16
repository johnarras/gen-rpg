using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapMods.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MapMods.Services
{
    public interface IMapModService : ISetupService
    {
        void Process(ServerGameState gs, MapMod mapMod);
    }
}
