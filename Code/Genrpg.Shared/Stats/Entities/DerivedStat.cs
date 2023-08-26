using MessagePack;
namespace Genrpg.Shared.Stats.Entities
{

    /// <summary>
    /// This tells how one stat affects another.
    /// For example, 1 int -> 2 magic damage
    /// 
    /// These will never be checked for loops, and all of them will be processed
    /// in sequence so designers will have to make sure that they enter
    /// them in the correct order. This may be fixed some day if it
    /// gets too cumbersome
    /// 
    /// 
    /// Formula: ToStatVal += FromStatVal*Points
    /// </summary>
    [MessagePackObject]
    public class DerivedStat
    {
        /// <summary>
        /// Stat sending points
        /// </summary>
        [Key(0)] public long FromStatTypeId { get; set; }
        /// <summary>
        /// Stat receiving points
        /// </summary>
        [Key(1)] public long ToStatTypeId { get; set; }

        /// <summary>
        /// Percent is used here so we could have 50pct of a stat going to another stat if we wanted.
        /// </summary>
        [Key(2)] public int Percent { get; set; }
    }
}
