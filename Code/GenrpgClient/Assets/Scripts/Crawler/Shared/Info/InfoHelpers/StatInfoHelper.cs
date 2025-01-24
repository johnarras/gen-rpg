using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class StatInfoHelper : SimpleInfoHelper<StatSettings, StatType>
    {

        public override long GetKey() { return EntityTypes.Stat; }

    }
}
