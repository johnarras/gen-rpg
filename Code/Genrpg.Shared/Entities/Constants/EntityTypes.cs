using Genrpg.Shared.Characters.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Entities.Constants
{
    public class EntityTypes
    {
        public const long None = 0;
        public const long Currency = 1;
        public const long Item = 2;
        public const long Spell = 3;
        public const long Unit = 4;
        public const long Spawn = 5;
        public const long Scaling = 7;
        public const long Recipe = 8;
        public const long Quest = 9;
        public const long Set = 10;
        public const long QuestItem = 12;
        public const long GroundObject = 13;
        public const long Zone = 14;
        public const long ZoneUnit = 15;
        public const long ProxyCharacter = 16;
        public const long Building = 17;
        public const long MapMod = 18;
        public const long Tribe = 19;
        public const long Reputation = 20;
        public const long Chest = 21;
        public const long BoardPrize = 22;
        public const long BoardMode = 23;
        public const long Vulnerability = 24;
        public const long Resist = 25;


        public const long Stat = 31;
        public const long StatPct = 32;
        public const long Damage = 33;
        public const long Healing = 34;
        public const long Shield = 35;
        public const long StatusEffect = 36;
        public const long SpecialMagic = 37;
        public const long Attack = 38;
        public const long Shoot = 39;
        public const long Riddle = 40;

        // Crawler-specific entitytypes
        public const long CrawlerSpell = 100;
        public const long PartyBuff = 101;
        public const long Map = 102;

        // User Reward Types
        public const long UserCoin = 200;
    }
}
