using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    [MessagePackObject]
    public class PartySummon
    { 
        [Key(0)] public string Id { get; set; }
        [Key(1)] public long UnitTypeId { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public long RoleId { get; set; }
    }
}
