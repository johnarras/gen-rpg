using Genrpg.Shared.MapMessages;
using Genrpg.Shared.MapMessages.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Movement.Messages
{
    [MessagePackObject]
    public sealed class OnAddToGrid : BaseMapApiMessage
    {
        [Key(0)] public string UserId { get; set; }
        [Key(1)] public int GridX { get; set; }
        [Key(2)] public int GridZ { get; set; }
    }
}
