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
    public interface IUnitGenService : IInjectable
    {
        string GenerateUnitPrefixName(IRandom rand, long unitTypeId, Zone zone,
            Dictionary<string, string> args = null);

        UnitType GetRandomUnitType(IRandom rand, Map map, Zone zone);

        string GenerateUnitName(IRandom rand, long unitTypeId, long zoneId,
            Dictionary<string, string> args = null);

    }

}
