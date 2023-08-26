using MessagePack;
namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class AbilityRank
    {
        [Key(0)] public long AbilityCategoryId { get; set; }
        [Key(1)] public long AbilityTypeId { get; set; }
        [Key(2)] public int Rank { get; set; }
    }
}
