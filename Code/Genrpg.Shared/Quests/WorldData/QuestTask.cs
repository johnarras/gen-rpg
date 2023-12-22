using MessagePack;
namespace Genrpg.Shared.Quests.WorldData
{
    [MessagePackObject]
    public class QuestTask
    {
        [Key(0)] public int Index { get; set; }
        [Key(1)] public long TaskEntityTypeId { get; set; }
        [Key(2)] public long TaskEntityId { get; set; }
        [Key(3)] public long OnEntityTypeId { get; set; }
        [Key(4)] public long OnEntityId { get; set; }
        [Key(5)] public long Quantity { get; set; }
        [Key(6)] public float DropChance { get; set; }
        [Key(7)] public int MinDrop { get; set; }
        [Key(8)] public int MaxDrop { get; set; }
        [Key(9)] public string Name { get; set; }
        [Key(10)] public string Icon { get; set; }


    }
}
