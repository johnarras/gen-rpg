using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Entities.Constants
{

    [MessagePackObject]
    public class EntityType : IIndexedGameItem
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
        public const long NPC = 11;
        public const long QuestItem = 12;
        public const long GroundObject = 13;
        public const long Zone = 14;
        public const long ZoneUnit = 15;
        public const long ProxyCharacter = 16;

        public const long Stat = 31;
        public const long StatPct = 32;
        public const long Damage = 33;
        public const long Healing = 34;
        public const long Shield = 35;


        // User Reward Types
        public const long UserCoin = 200;

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }
    }
}
