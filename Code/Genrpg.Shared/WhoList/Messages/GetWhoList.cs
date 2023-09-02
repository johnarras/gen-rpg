using MessagePack;
using Genrpg.Shared.MapMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.WhoList.Messages
{
    [MessagePackObject]
    public sealed class GetWhoList : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string Args { get; set; }
    }
}
