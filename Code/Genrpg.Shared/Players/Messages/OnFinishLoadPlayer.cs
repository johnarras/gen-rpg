using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Players.Messages
{
    [MessagePackObject]
    public sealed class OnFinishLoadPlayer : BaseMapApiMessage
    {
    }
}
