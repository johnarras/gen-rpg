using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Constants
{
    public class TargetTypes
    {

        public const int None = 0;
        public const int Enemy = 1; // Can be cast on enemy, can have parts that are Ally....either hit nearby ally randomly or hit self.
        public const int Ally = 2; // Can be cast on self or others, can have Enemy parts that hit things nearby.
    }
}
