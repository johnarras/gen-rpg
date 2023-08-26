using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.NPCs.Messages
{
    [MessagePackObject]
    public sealed class GetNPCStatus : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string UnitId { get; set; }
    }
}
