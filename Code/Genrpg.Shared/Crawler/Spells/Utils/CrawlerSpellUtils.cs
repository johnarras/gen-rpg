using Genrpg.Shared.Spells.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Spells.Utils
{
    public class CrawlerSpellUtils
    {

        public static bool IsEnemyTarget(long targetTypeId)
        {
            return targetTypeId == TargetTypes.Enemy ||
                targetTypeId == TargetTypes.EnemyGroup ||
                targetTypeId == TargetTypes.AllEnemies ||
                targetTypeId == TargetTypes.AllEnemyGroups;
        }

        public static bool IsNonCombatTarget(long targetTypeId)
        {
            return targetTypeId == TargetTypes.Item ||
                targetTypeId == TargetTypes.Special ||
                targetTypeId == TargetTypes.Location;
        }

    }
}
