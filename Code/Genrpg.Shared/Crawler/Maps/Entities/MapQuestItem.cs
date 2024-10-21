using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class MapQuestItem
    {
        [Key(0)] public long QuestItemId { get; set; }
        [Key(1)] public long Quantity { get; set; }
        [Key(2)] public long FoundInMapId { get; set; }
    }
}
