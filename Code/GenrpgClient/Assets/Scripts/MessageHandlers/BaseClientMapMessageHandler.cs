using Genrpg.Shared.DataStores.Entities;
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

    protected abstract void InnerProcess(UnityGameState gs, T msg, CancellationToken token);

    public void Process(UnityGameState gs, IMapApiMessage msg, CancellationToken token)
    {
        InnerProcess(gs, msg as T, token);
    }
}

