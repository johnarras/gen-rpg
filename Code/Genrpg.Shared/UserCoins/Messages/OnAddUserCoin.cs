using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UserCoins.Messages
{
    [MessagePackObject]
    public sealed class OnAddUserCoin : BaseMapApiMessage
    {
        [Key(0)] public string CharId { get; set; }
        [Key(1)] public long UserCoinTypeId { get; set; }
        [Key(2)] public long QuantityAdded { get; set; }
    }
}
