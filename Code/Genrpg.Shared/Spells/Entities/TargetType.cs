using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Spells.Entities
{
    /// <summary>
    /// What kind of target a spell has.
    /// 
    /// When crafting spells, Buffs can only be added to other buffs.
    /// But spells with Ally+Enemy parts can both be combined. (like damage+heal)
    /// 
    /// 
    /// </summary>
    [MessagePackObject]
    public class TargetType : IIndexedGameItem
    {

        public const int None = 0;
        public const int Enemy = 1; // Can be cast on enemy, can have parts that are Ally....either hit nearby ally randomly or hit self.
        public const int Ally = 2; // Can be cast on self or others, can have Enemy parts that hit things nearby.


        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }
    }
}
