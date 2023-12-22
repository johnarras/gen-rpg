using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Inventory.Messages
{
    [MessagePackObject]
    public sealed class OnUpdateItem : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
        [Key(1)] public Item Item { get; set; }
    }
}
