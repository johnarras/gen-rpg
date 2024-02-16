using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UnitEffects.Constants
{
    public class StatusEffects
    {
        public const int Poisoned = 1; // Take damage over time
        public const int Diseased = 2; // Lower stats
        public const int Cursed = 3; // Lower effective level
        public const int Paralyzed = 4; // Cannot do anything
        public const int Stunned = 5; // Cannot do anything
        public const int Possessed = 6; // Sometimes attack allies
        public const int Insane = 7; // IDK
        public const int FeebleMind = 8; // Very weak
        public const int Petrified = 9; // Cannot do anything
        public const int Silenced = 10; // Cannot cast
        public const int Rooted = 11; // Cannot move/attack
        public const int Feared = 12; // Cannot do anything
        public const int Hidden = 13; // Special state to allow special attacks
        public const int Taunt = 14; // Monsters attack you
        public const int Slowed = 15; // Slower attacks/speed
        public const int Hasted = 16; // Faster attacks
        public const int Weak = 17; // Less dam
        public const int Multiply = 18; // Next spell to heal/dam is multiplied 
        public const int Dead = 19;

    }
}
