using MessagePack;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Trades.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnUpdateTrade : BaseMapApiMessage
    {
        [Key(0)] public TradeObject TradeObject { get; set; }
    }
}
