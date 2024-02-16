using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Combat.Utils
{
    public class CombatUtils
    {
        private static bool TotallyIncapacitated(int flagbits)
        {
            return FlagUtils.IsSet(flagbits,
                    (1 << StatusEffects.Dead |
                    1 << StatusEffects.Paralyzed |
                    1 << StatusEffects.Feared |
                    1 << StatusEffects.Petrified |
                    1 << StatusEffects.Stunned));
        }

        public static bool CanPerformAction(CrawlerUnit unit)
        {
            if (unit.StatusEffects.Data.Count < 1)
            {
                return true;
            }

            int firstEffects = unit.StatusEffects.Data[0].Bits;

            return !TotallyIncapacitated(firstEffects);
        }
    }
}
