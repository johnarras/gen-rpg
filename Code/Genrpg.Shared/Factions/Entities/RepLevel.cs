using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Factions.Entities
{
    [MessagePackObject]
    public class RepLevel : IIndexedGameItem
    {

        public const int None = 0;
        public const int Hated = 1;
        public const int Hostile = 2;
        public const int Unfriendly = 3;
        public const int Neutral = 4;
        public const int Friendly = 5;
        public const int Honored = 6;
        public const int Revered = 7;
        public const int Exalted = 8;

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }

        [Key(4)] public string Art { get; set; }

        [Key(5)] public int PointsNeeded { get; set; }
    }

}
