using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Messages
{
    [MessagePackObject]
    public sealed class UnequipItem : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string ItemId { get; set; }
    }
}
