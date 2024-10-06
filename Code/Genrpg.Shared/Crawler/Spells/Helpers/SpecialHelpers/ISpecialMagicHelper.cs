using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;

namespace Genrpg.Shared.Crawler.Spells.Helpers.SpecialHelpers
{
    public interface ISpecialMagicHelper : ISetupDictionaryItem<long>
    {
        Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData, SelectSpellAction action,
            CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token);
    }
}
