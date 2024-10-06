using Genrpg.Shared.Client.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Threading;

public abstract class BaseClientMapMessageHandler<T> : IClientMapMessageHandler where T : class, IMapApiMessage
{
    public Type GetKey() { return typeof(T); }

    protected IClientMapObjectManager _objectManager;
    protected IMapTerrainManager _terrainManager;
    protected IRepositoryService _repoService;
    protected ILogService _logService;
    protected IDispatcher _dispatcher;
    protected CancellationToken _token;
    protected IClientGameState _gs;
    protected IClientRandom _rand;

    protected abstract void InnerProcess(T msg, CancellationToken token);

    public void Process(IMapApiMessage msg, CancellationToken token)
    {
        InnerProcess(msg as T, token);
    }
}

