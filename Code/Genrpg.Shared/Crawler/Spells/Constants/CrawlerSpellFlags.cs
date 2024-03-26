using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Spells.Constants
{
    public class CrawlerSpellFlags
    {
        public const int SuppressCastText = (1 << 0);
        public const int MonsterOnly = (1 << 1);
    }
}
