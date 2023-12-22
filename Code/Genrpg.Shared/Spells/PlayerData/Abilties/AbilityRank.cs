using Genrpg.Shared.DataStores.Categories.PlayerData;
using MessagePack;
namespace Genrpg.Shared.Spells.PlayerData.Abilties
{
    [MessagePackObject]
    public class AbilityRank : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long AbilityCategoryId { get; set; }
        [Key(3)] public long AbilityTypeId { get; set; }
        [Key(4)] public int Rank { get; set; }
    }
}
