using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    [MessagePackObject]
    public class PartyQuestItem
    {
        [Key(0)] public long CrawlerQuestItemId { get; set; }
        [Key(1)] public long Quantity { get; set; }
    }
}
