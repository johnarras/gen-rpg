using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Quests.Messages
{
    [MessagePackObject]
    public sealed class GetNPCQuests : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public long NPCTypeId { get; set; }
    }
}
