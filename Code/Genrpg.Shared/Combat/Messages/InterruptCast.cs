using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Combat.Messages
{
    [MessagePackObject]
    public sealed class InterruptCast : BaseMapApiMessage, IPlayerCommand
    {
    }
}
