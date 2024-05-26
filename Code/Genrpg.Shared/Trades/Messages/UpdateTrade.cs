using MessagePack;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Trades.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Trades.Constants;

namespace Genrpg.Shared.Trades.Messages
{
    [MessagePackObject]
    public sealed class UpdateTrade : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string[] ItemIds { get; set; } = new string[TradeConstants.MaxItems];
        [Key(1)] public long Money { get; set; }
    }
}
