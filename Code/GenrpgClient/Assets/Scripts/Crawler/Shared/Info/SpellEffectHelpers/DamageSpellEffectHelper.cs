using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.SpellEffectHelpers
{
    public class DamageSpellEffectHelper : BaseNumericSpellEffectHelper
    {
        public override long GetKey() { return EntityTypes.Damage; }
    }
}
