using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class AbilityCategory : IIndexedGameItem
    {

        public const int Element = 1;
        public const int Skill = 2;

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Abbrev { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Icon { get; set; }
        [Key(5)] public string Art { get; set; }
    }
}
