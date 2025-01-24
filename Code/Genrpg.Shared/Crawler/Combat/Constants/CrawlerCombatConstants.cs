using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Combat.Constants
{
    public class CrawlerCombatConstants
    {
        public const int MinRange = 10;
        public const int MaxRange = 100;
        public const int RangeDelta = 10;

        public const int MaxStartEnemyGroupCount = 6;

        public const int BaseAttackQuantity = 1;
        public const int BasePlayerAttackQuantity = 2;

        public const long BaseMinDamage = 1;
        public const long BaseMaxDamage = 2;

        public const int PartyCombatGroupIndex = 0;

        public const long SelfSummonPlaceholderId = -1;
        public const long BaseSummonPlaceholderId = -2;
    }
}
