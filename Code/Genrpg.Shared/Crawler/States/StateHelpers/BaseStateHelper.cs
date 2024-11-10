using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading;
using Genrpg.Shared.Crawler.Maps.Services;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.UI.Services;

namespace Genrpg.Shared.Crawler.States.StateHelpers
{
    public abstract class BaseStateHelper : IStateHelper
    {

        protected ICrawlerService _crawlerService;
        protected ICrawlerStatService _statService;
        protected ICrawlerCombatService _combatService;
        protected ICrawlerSpellService _spellService;
        protected ILogService _logService;
        protected IRepositoryService _repoService;
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected IClientRandom _rand;
        protected ICrawlerWorldService _worldService;
        protected IDispatcher _dispatcher;
        protected ITextService _textService;

        public abstract ECrawlerStates GetKey();
        public abstract Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token);

        public virtual bool IsTopLevelState() { return false; }
        public virtual long TriggerBuildingId() { return 0; }
        public virtual long TriggerDetailEntityTypeId() { return 0; }

        virtual protected CrawlerStateData CreateStateData()
        {
            return new CrawlerStateData(GetKey());
        }
    }
}
