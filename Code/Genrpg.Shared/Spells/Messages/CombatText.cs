using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class CombatText : BaseMapApiMessage
    {
        [Key(0)] public string TargetId { get; set; }
        [Key(1)] public string Text { get; set; }
        [Key(2)] public int TextColor { get; set; }
        [Key(3)] public bool IsCrit { get; set; }
    }
}
