using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.ProcGen.RandomNumbers;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers
{
    public abstract class BaseStateHelper : IStateHelper
    {

        protected ICrawlerService _crawlerService;
        protected ICrawlerStatService _statService;
        protected ICombatService _combatService;
        protected ICrawlerSpellService _spellService;
        protected ILogService _logService;
        protected IRepositoryService _repoService;
        protected IGameData _gameData;
        protected IUnityGameState _gs;
        protected IClientRandom _rand;
        protected ICrawlerWorldService _worldService;

        public abstract ECrawlerStates GetKey();
        public abstract Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token);

        public virtual bool IsTopLevelState() { return false; }

        virtual protected CrawlerStateData CreateStateData()
        {
            return new CrawlerStateData(GetKey());
        }
    }
}
