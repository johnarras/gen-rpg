using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.GameEvents
{
    public class ShowPartyMinimap
    {
        public PartyData Party { get; set; }
        public bool PartyArrowOnly { get; set; }
    }
}
