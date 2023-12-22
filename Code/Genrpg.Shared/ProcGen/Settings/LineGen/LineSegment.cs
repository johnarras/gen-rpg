using MessagePack;
namespace Genrpg.Shared.ProcGen.Settings.LineGen
{
    [MessagePackObject]
    public class LineSegment
    {
        [Key(0)] public float SX { get; set; }
        [Key(1)] public float SY { get; set; }
        [Key(2)] public float EX { get; set; }
        [Key(3)] public float EY { get; set; }
    }
}
