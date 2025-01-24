
using Assets.Scripts.Awaitables;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;

namespace Assets.Scripts.Login.Messages.Core
{
    public abstract class BaseClientLoginResultHandler<T> : IClientLoginResultHandler where T : class, IWebResponse
    {
        protected ILogService _logService;
        protected IRepositoryService _repoService;
        protected IDispatcher _dispatcher;
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected IAwaitableService _awaitableService;
        public Type GetKey() { return typeof(T); }

        virtual public int Priority() { return 0; }

        protected abstract void InnerProcess(T result, CancellationToken token);

        public void Process(IWebResponse result, CancellationToken token)
        {
            InnerProcess(result as T, token);
        }
    }
}
