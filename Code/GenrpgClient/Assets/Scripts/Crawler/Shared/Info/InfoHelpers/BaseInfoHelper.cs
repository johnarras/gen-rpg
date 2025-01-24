using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.GameSettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public abstract class BaseInfoHelper<TParent, TChild> : IInfoHelper where TParent : ParentSettings<TChild> where TChild : ChildSettings, new()
    {

        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected ICrawlerStatService _statService;
        protected IEntityService _entityService;
        protected IInfoService _infoService;


        public abstract long GetKey();
        public abstract List<string> GetInfoLines(long entityId);
        public virtual string GetTypeName() { return typeof(TChild).Name; }
    }
}
