using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Services
{
    public interface IUnitGenService : IService
    {
        string GenerateUnitPrefixName(GameState gs, long unitTypeId, Zone zone, IRandom rand,
            Dictionary<string, string> args = null);

        UnitType GetRandomUnitType(GameState gs, Map map, Zone zone);

        string GenerateUnitName(GameState gs, long unitTypeId, long zoneId, IRandom rand,
            Dictionary<string, string> args = null);

    }

}
