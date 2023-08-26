using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class OnSetActionBarItem : BaseMapApiMessage
    {
        [Key(0)] public long SpellId { get; set; }
        [Key(1)] public int Index { get; set; }
    }
}
