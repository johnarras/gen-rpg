using MessagePack;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.GameEvents
{
    [MessagePackObject]
    public class ShowPartyMinimap
    {
        [Key(0)] public PartyData Party { get; set; }
        [Key(1)] public bool PartyArrowOnly { get; set; }
    }
}
