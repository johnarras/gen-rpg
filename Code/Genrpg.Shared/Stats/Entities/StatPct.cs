using MessagePack;
namespace Genrpg.Shared.Stats.Entities
{

    public interface IStatPct
    {
        long StatTypeId { get; set; }
        int Percent { get; set; }
    }

    [MessagePackObject]
    public class StatPct : IStatPct
    {
        [Key(0)] public long StatTypeId { get; set; }
        [Key(1)] public int Percent { get; set; }
    }
}
