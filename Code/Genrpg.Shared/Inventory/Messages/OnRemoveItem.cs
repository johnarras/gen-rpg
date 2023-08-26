using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Messages
{
    [MessagePackObject]
    public sealed class OnRemoveItem : BaseMapApiMessage
    {
        [Key(0)] public string ItemId { get; set; }
        [Key(1)] public string UnitId { get; set; }
    }
}
