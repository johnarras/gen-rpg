using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public interface ISpecialMagicHelper : ISetupDictionaryItem<long>
    {
        Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData, SelectSpellAction action,
            CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token);
    }
}
