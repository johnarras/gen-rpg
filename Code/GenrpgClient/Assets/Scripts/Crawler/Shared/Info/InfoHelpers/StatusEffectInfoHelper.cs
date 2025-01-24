using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.UnitEffects.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class StatusEffectInfoHelper : SimpleInfoHelper<StatusEffectSettings, StatusEffect>
    {

        public override long GetKey() { return EntityTypes.StatusEffect; }


    }
}
