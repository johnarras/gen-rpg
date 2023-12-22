using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;

namespace Genrpg.Shared.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class OnGetMapObjectStatus : BaseMapApiMessage
    {
        [Key(0)] public string ObjId { get; set; }
        [Key(1)] public List<IMapObjectAddon> Addons { get; set; } = new List<IMapObjectAddon>();
    }
}
