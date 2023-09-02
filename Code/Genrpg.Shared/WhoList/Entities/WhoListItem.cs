using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.WhoList.Entities
{
    [MessagePackObject]
    public class WhoListItem
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public long Level { get; set; }
        [Key(3)] public string ZoneName { get; set; }
    }
}
