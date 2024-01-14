using MessagePack;
using Genrpg.Shared.MapMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Ftue.Messages
{
    [MessagePackObject]
    public sealed class CompleteFtueStepMessage : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public long FtueStepId { get; set; }
    }
}
