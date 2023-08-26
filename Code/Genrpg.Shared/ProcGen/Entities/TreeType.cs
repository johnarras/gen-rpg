using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.ProcGen.Entities
{
    /// <summary>
    /// Data about a particular tree type used in terrain generator
    /// </summary>
    /// 
    public class TreeFlags
    {
        public const int IsBush = 1 << 0;
        public const int IsWaterItem = 1 << 1;
        public const int NoNearbyItems = 1 << 2;
        public const int DirectPlaceObject = 1 << 3;
    }

    [MessagePackObject]
    public class TreeType : IIndexedGameItem
    {
        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }

        [Key(5)] public float Scale { get; set; } = 1.0f;

        [Key(6)] public int VariationCount { get; set; } = 1;

        [Key(7)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }
        public TreeType()
        {
        }
    }
}
