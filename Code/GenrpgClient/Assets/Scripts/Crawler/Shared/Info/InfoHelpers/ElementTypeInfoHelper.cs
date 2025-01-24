using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Stats.Settings.Stats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class ElementTypeInfoHelper : SimpleInfoHelper<ElementTypeSettings, ElementType>
    {

        public override long GetKey() { return EntityTypes.Element; }

    }
}
