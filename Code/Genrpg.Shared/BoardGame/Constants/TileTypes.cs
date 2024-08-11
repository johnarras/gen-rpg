using Genrpg.Shared.Characters.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Constants
{
    public class TileTypes
    {
        public const long Gold = 1;
        public const long Home = 2;
        public const long Gem = 3;
        public const long Dice = 4;
        public const long PVP = 5; // Attack a player
        public const long DrawCard = 6; // Roll on a loot table
        public const long GuardTower = 7; // Get more guard towers
        public const long PathEntrance = 8; // Go to sidepath
        public const long Portal = 9; // Go to another board
        public const long Bonus = 10; // Get bonus rolls on your board
        public const long Defend = 11; // Monsters attack you
        public const long Chest = 12; // Unlock chests tile
        public const long TeleportStart = 13;
        public const long TeleportEnd = 14;
        public const long SidePath = 15;
    }
}
