using Genrpg.Shared.Inventory.PlayerData;
using MessagePack;
namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class UseItemResult
    {
        [Key(0)] public bool Success { get; set; }
        [Key(1)] public Item ItemUsed { get; set; }
        [Key(2)] public object ResultObject { get; set; }
        [Key(3)] public string Message { get; set; }
    }
}
