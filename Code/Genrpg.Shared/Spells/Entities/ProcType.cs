using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class ProcType : IIndexedGameItem
    {
        public const int OnCast = 1;
        public const int OnHitTarget = 2;
        public const int OnWasHit = 3;
        public const int OnDeath = 4;


        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }

        [Key(4)] public string Art { get; set; }
    }
}
