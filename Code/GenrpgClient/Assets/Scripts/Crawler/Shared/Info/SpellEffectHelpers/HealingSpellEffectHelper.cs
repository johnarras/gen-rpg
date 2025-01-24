using Genrpg.Shared.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.SpellEffectHelpers
{
    public class HealingSpellEffectHelper : BaseNumericSpellEffectHelper
    {
        public override long GetKey() { return EntityTypes.Healing; }
    }
}
