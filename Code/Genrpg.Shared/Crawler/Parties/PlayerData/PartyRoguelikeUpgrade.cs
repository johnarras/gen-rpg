using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    [MessagePackObject]
    public class PartyRoguelikeUpgrade
    {
        [Key(0)] public long RoguelikeUpgradeId { get; set; }
        [Key(1)] public long Tier { get; set; }
    }
}
