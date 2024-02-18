using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Genrpg.Shared.Stats.Constants
{
    public class StatTypes
    {
        public const int None = 0;
        public const int Health = 1;
        public const int Mana = 2; // slow drain, slow regen.
        public const int Energy = 3; // quick drain, quick regen. 
        public const int Combo = 4; // Build up with use of other skills

        public const int Strength = 11; // + melee dam
        public const int Intellect = 12; // + spell points
        public const int Agility = 13; // + ranged dam
        public const int Stamina = 14; // health
        public const int Luck = 15; // Does???

        // For 20-60 make sure the offsets for +power, +defense, +powerMult , +defenseMult are all offset the same at 2x 3x 4x 5x
        // to simplify calculations.

        public const int Power = 20; // All Attack
        public const int AttackPower = 21; // Physical Attack
        public const int SpellPower = 22; // Magical Attack

        public const int Defense = 30; // All Defense
        public const int Armor = 31; // Resist phys Armor
        public const int Resist = 32; // Resist magic Resist

        public const int PowerMult = 40; // multiplier to all dam/healing
        public const int AttackMult = 41; // Multiplier to body dam/healing
        public const int SpellMult = 42; // Multiplier to magic dam/healing

        public const int DefenseMult = 50; // All Defense Mult
        public const int AttackDefMult = 51; // Physical Defense Mult
        public const int SpellDefMult = 52; // Magical Defense Mult


        public const int Crit = 61;
        public const int Haste = 62;
        public const int Speed = 63; // Move speed
        public const int Efficiency = 64; // Cost reduction
        public const int Cooldown = 65; // Cooldown reduction
        public const int CritDam = 66; // Crit damage
        
        public const int Hit = 70; // Chance to hit
        public const int Leadership = 71; // effective attack level
    }
}
