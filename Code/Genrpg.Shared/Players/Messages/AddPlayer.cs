using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Players.Messages
{
    [MessagePackObject]
    public sealed class AddPlayer : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string UserId { get; set; }
        [Key(1)] public string CharacterId { get; set; }
        [Key(2)] public string SessionId { get; set; }
    }
}
