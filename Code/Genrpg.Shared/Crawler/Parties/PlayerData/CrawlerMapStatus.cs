using MessagePack;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    [MessagePackObject]
    public class CrawlerMapStatus
    {
        [Key(0)] public long MapId { get; set; }
        [Key(1)] public int CellsVisited { get; set; }
        [Key(2)] public int TotalCells { get; set; }
        [Key(3)] public SmallIndexBitList Visited { get; set; } = new SmallIndexBitList();
    }
}
