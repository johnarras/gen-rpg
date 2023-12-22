using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData;

namespace Genrpg.Shared.Input.PlayerData
{
    [MessagePackObject]
    public class ActionInput : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public int Index { get; set; }
        [Key(3)] public long SpellId { get; set; }
    }


}
