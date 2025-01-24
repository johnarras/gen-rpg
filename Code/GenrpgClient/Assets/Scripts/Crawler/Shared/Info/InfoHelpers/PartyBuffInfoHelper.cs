using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class PartyBuffInfoHelper : SimpleInfoHelper<PartyBuffSettings, PartyBuff>
    {

        public override long GetKey() { return EntityTypes.PartyBuff; }

    }
}
