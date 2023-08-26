using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Pings.Messages
{
    [MessagePackObject]
    public sealed class Ping : BaseMapApiMessage
    {
    }
}
