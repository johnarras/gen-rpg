using Genrpg.Shared.Crawler.Monsters.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Stats.Utils
{
    public static class CrawlerStatUtils
    {
        public static long GetStatBonus(CrawlerUnit unit, long statTypeId)
        {
            if (statTypeId < 1)
            {
                return 0;
            }

            long statValue = unit.Stats.Max(statTypeId);

            long baseBonus = 0;

            if (statValue < 13)
            {
                baseBonus = 0;
            }
            else if (statValue < 16)
            {
                baseBonus = 1;
            }
            else if (statValue < 18)
            {
                baseBonus = 2;
            }
            else if (statValue <= 20) // 18-20 go from 3 to 5
            {
                baseBonus = (statValue - 18) + 3;
            }
            else if (statValue <= 50) // 21-50 go from 6 to 11
            {
                baseBonus = 5 + (statValue - 20) / 5;
            }
            else // 11 at 50 then 1 point per 10 stat vals after. Cap at 250 for 11 + 11 = 22
            {
                if (statValue > 250)
                {
                    statValue = 250;
                }

                baseBonus = 11 + (statValue - 50) / 10;
            }

            return baseBonus;

        }
    }
}
