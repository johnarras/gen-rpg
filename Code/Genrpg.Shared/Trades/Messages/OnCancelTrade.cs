using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnCancelTrade : BaseMapApiMessage
    {
        [Key(0)] public string CharId { get; set; }
        [Key(1)] public string ErrorMessage { get; set; }
    }
}
