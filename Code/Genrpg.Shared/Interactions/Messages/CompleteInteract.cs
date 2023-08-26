using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Interactions.Messages
{
    [MessagePackObject]
    public sealed class CompleteInteract : BaseMapApiMessage, ITargetMessage
    {
        [Key(0)] public string CasterId { get; set; }
        [Key(1)] public string TargetId { get; set; }
        [Key(2)] public long Level { get; set; }
        [Key(3)] public long CrafterTypeId { get; set; }
        [Key(4)] public int SkillPoints { get; set; }
        [Key(5)] public long GroundObjTypeId { get; set; }
        [Key(6)] public bool IsSkillLoot { get; set; }
    }
}
