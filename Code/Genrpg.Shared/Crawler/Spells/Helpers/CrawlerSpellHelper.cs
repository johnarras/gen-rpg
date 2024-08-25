using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Spells.Helpers
{
    public class CrawlerSpellHelper : BaseEntityHelper<CrawlerSpellSettings, CrawlerSpell>
    {
        public override long GetKey() { return EntityTypes.CrawlerSpell; }
    }
}
