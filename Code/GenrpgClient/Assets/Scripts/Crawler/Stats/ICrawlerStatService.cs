using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Stats.Services
{
    public interface ICrawlerStatService : ISetupService
    {
        void CalcUnitStats(GameState gs, PartyData party, CrawlerUnit unit, bool resetCurrStats);

        void CalcPartyStats(GameState gs, PartyData party, bool resetCurrStats);

        /// <summary>
        /// Get buff stats based on classes and levels of party members
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="party"></param>
        /// <returns></returns>
        List<Stat> GetPartyBuffStats(GameState gs, PartyData party);
    }
}
