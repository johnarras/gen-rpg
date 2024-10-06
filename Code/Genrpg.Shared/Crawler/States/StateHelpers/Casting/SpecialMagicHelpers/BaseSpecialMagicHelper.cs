using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Services;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public abstract class BaseSpecialMagicHelper : ISpecialMagicHelper
    {
        protected IDispatcher _dispatcher;
        protected IGameData _gameData;
        protected ICrawlerService _crawlerService;
        protected ICrawlerCombatService _combatService;
        protected ICrawlerMapService _mapService;
        protected ICrawlerWorldService _worldService;
        protected ILogService _logService;
        protected ICrawlerSpellService _spellService;
        protected ITextService _textService;

        public abstract long GetKey();

        public abstract Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData, SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect,
            CancellationToken token);
    }
}
