using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UnitEffects.Constants
{
    public class StatusEffects
    {
        public const int Poisoned = 1; // No health regen, some DoT
        public const int Diseased = 2; // No mana regen, some Mana Loss Over Time

        public const int Cursed = 3; // Lower effective level
        public const int Slowed = 4; // Slower attacks/speed

        public const int Weak = 5; // Less physical dam
        public const int FeebleMind = 6; // Less magical dam
        public const int Jittery = 7; // Less ranged dam

        public const int Silenced = 8; // Cannot cast
        public const int Rooted = 9; // Cannot melee
        public const int Nearsighted = 10; // Cannot shoot

        public const int Feared = 11; // Cannot do anything
        public const int Stunned = 12; // Cannot do anything
        public const int Petrified = 13; // Cannot do anything

        public const int Possessed = 14; // Attack allies?

        public const int Hidden = 15; // Special state to allow special attacks
        public const int Taunt = 16; // Monsters attack you
        public const int Hasted = 17; // Faster attacks
        public const int Multiply = 18; // Next spell to heal/dam is multiplied 
        public const int Dead = 19;

    }
}
