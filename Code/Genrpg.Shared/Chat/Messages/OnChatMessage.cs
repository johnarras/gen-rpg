using MessagePack;
using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.Chat.Messages
{
    [MessagePackObject]
    public sealed class OnChatMessage : BaseMapApiMessage
    {
        [Key(0)] public string SenderId { get; set; }
        [Key(1)] public string SenderName { get; set; }
        [Key(2)] public long ChatTypeId { get; set; }
        [Key(3)] public string Message { get; set; }
    }
}
