using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Factions.Entities
{
    [MessagePackObject]
    public class FactionType : IIndexedGameItem
    {
        public const int Player = 0;
        public const int Faction1 = 1;
        public const int Faction2 = 2;

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }

        [Key(4)] public string Art { get; set; }

        [Key(5)] public int StartRepLevelId { get; set; }

    }
}
