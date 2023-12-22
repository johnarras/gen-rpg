using MessagePack;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Crafting.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.Charms.PlayerData
{
    [MessagePackObject]
    public class PlayerCharm : OwnerPlayerData, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public string Hash { get; set; }
        [Key(4)] public long CurrentCharmUseId { get; set; }
        [Key(5)] public string TargetId { get; set; }
        [Key(6)] public string TargetName { get; set; }

        [Key(7)] public List<PlayerCharmBonusList> Bonuses { get; set; } = new List<PlayerCharmBonusList>();

    }
}
