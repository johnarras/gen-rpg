using MessagePack;
using Genrpg.Shared.Quests.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.Quests.Messages
{
    [MessagePackObject]
    public sealed class OnGetNPCQuests : BaseMapApiMessage
    {
        [Key(0)] public long NPCTypeId { get; set; }
        [Key(1)] public List<QuestType> Quests { get; set; }
    }
}
