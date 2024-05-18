using MessagePack;
namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class SamplingData
    {
        [Key(0)] public double XMin { get; set; }
        [Key(1)] public double XMax { get; set; }
        [Key(2)] public double YMin { get; set; }
        [Key(3)] public double YMax { get; set; }
        [Key(4)] public double ZMin { get; set; }
        [Key(5)] public double ZMax { get; set; }
        [Key(6)] public int Count { get; set; }
        [Key(7)] public double MinSeparation { get; set; }
        [Key(8)] public int MaxAttemptsPerItem { get; set; }
        [Key(9)] public long Seed { get; set; }
        [Key(10)] public float NoiseAmp { get; set; } = 0.0f;
        [Key(11)] public float NoiseFreq { get; set; } = 1.0f;
    }
}
