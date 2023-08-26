using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Chat.Messages
{
    [MessagePackObject]
    public sealed class SendChatMessage : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public long ChatTypeId { get; set; }
        [Key(1)] public string ToId { get; set; }
        [Key(2)] public string Text { get; set; }
    }
}
