using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Interactions.Messages
{
    [MessagePackObject]
    public sealed class InteractCommand : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string TargetId { get; set; }
        [Key(1)] public bool IsSkillLoot { get; set; }
    }
}
