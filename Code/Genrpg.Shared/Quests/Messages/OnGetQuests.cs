using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Quests.WorldData;

namespace Genrpg.Shared.Quests.Messages
{
    [MessagePackObject]
    public sealed class OnGetQuests : BaseMapApiMessage
    {
        [Key(0)] public string ObjId { get; set; }
        [Key(1)] public List<QuestType> Quests { get; set; }
    }
}
