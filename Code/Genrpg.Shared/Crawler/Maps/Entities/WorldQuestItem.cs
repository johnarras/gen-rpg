using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class WorldQuestItem : IIdName
    {
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public long FoundInMapId { get; set; }
        [Key(3)] public long UnlocksMapId { get; set; }
    }
}
