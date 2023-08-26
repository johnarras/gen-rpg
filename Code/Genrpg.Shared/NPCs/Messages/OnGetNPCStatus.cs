using MessagePack;
using Genrpg.Shared.NPCs.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.NPCs.Messages
{
    [MessagePackObject]
    public sealed class OnGetNPCStatus : BaseMapApiMessage
    {
        [Key(0)] public NPCStatus Status { get; set; }
        [Key(1)] public string UnitId { get; set; }
    }
}
