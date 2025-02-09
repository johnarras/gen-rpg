using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Constants
{
    public class TargetTypes
    {

        public const long None = 0;
        public const long Enemy = 1; // Can be cast on enemy, can have parts that are Ally....either hit nearby ally randomly or hit self.
        public const long Ally = 2; // Can be cast on self or others, can have Enemy parts that hit things nearby.
        public const long AllEnemiesInAGroup = 3;
        public const long AllEnemies = 4;
        public const long AllAllies = 5;
        public const long Location = 6;
        public const long Item = 7;
        public const long Special = 8;
        public const long Self = 9;
        public const long EnemyInEachGroup = 10;
    }
}
