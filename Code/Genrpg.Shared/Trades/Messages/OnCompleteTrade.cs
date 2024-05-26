using MessagePack;
using Genrpg.Shared.Trades.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnCompleteTrade : BaseMapApiMessage
    {
        [Key(0)] public TradeObject TradeObject { get; set; }
    }
}
