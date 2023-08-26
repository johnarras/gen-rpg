using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Stats.Messages
{
    [MessagePackObject]
    public sealed class Regen : BaseMapApiMessage
    {
    }
}
