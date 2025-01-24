using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Spells.Constants
{
    public class CrawlerSpellConstants
    {
        public const long StatBuffSpellIdOffset = 10000;


        public const long MonsterSummonSpellIdOffset = 20000;




        public const long MinPlaceholderSpellId = 30000;
        public const long SelfSummonPlaceholderSpellId = MinPlaceholderSpellId + 1;
        public const long BaseSummonPlaceholderSpellId = MinPlaceholderSpellId + 2;


        public const long MaxPlaceholderSpellId = 39999;
    }
}
