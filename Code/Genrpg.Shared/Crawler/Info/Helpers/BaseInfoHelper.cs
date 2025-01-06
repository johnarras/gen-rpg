using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.GameSettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.Helpers
{
    public abstract class BaseInfoHelper : IInfoHelper
    {

        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected ICrawlerStatService _statService;
        protected IEntityService _entityService;


        public abstract long GetKey();
        public abstract List<string> GetInfoLines(long entityId);
    }
}
