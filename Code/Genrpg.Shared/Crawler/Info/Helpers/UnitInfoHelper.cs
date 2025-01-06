using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Units.Settings;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.Helpers
{
    public class UnitInfoHelper : BaseInfoHelper
    {
        public override long GetKey() { return EntityTypes.Unit; }

        public override List<string> GetInfoLines(long entityId)
        {

            UnitType unitType = _gameData.Get<UnitSettings>(_gs.ch).Get(entityId);
            List<string> infoLines = new List<string>();

            infoLines.Add(unitType.Name);
            infoLines.Add(unitType.Desc);


            return infoLines;
        }

    }
}
