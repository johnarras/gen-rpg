using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class RemoveActionBarItem : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public int Index { get; set; }
    }
}
