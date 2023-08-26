using MessagePack;
namespace Genrpg.Shared.Quests.Entities
{
    [MessagePackObject]
    public class UpdateQuestData
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
    }
}
