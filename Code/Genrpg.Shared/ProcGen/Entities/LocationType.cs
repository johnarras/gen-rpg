using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class LocationType : IIndexedGameItem
    {
        public const int ZoneCenter = 1;
        public const int Secondary = 3;




        public const int MinSize = 4;

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }

        [Key(4)] public int XSize { get; set; }
        [Key(5)] public int YSize { get; set; }

        [Key(6)] public string SetupType { get; set; }
        [Key(7)] public string Art { get; set; }
    }
}
