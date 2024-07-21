using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Casting.SpecialMagicHelpers
{
    public abstract class BaseSpecialMagicHelper : ISpecialMagicHelper
    {
        protected IDispatcher _dispatcher;
        protected IGameData _gameData;
        protected ICrawlerService _crawlerService;
        protected ICombatService _combatService;
        protected ICrawlerMapService _mapService;
        protected ICrawlerWorldService _worldService;
        protected ILogService _logService;
        protected ICrawlerSpellService _spellService;

        public abstract long GetKey();

        public abstract Awaitable<CrawlerStateData> HandleEffect(CrawlerStateData stateData, SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect,
            CancellationToken token);
    }
}
