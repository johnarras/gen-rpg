using GEntity = UnityEngine.GameObject;
using System;

using Assets.Scripts.Interfaces;
using System.Threading;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.GameSettings;
using Assets.Scripts.ProcGen.RandomNumbers;
using Genrpg.Shared.Interfaces;

public class BaseBehaviour : StubComponent, IInitOnResolve
{
    protected IUnityUpdateService _updateService;
    protected IScreenService _screenService;
    protected IRealtimeNetworkService _networkService;
    protected IAssetService _assetService;
    protected IUIService _uIInitializable;
    protected ILogService _logService;
    protected IDispatcher _dispatcher;
    protected IGameData _gameData;
    protected IUnityGameState _gs;
    protected IClientRandom _rand;
    protected IGameObjectService _gameObjectService;

    public CancellationToken GetToken()
    {
        return this.GetCancellationToken();
    }


    public virtual void Init()
    {

    }
    public GEntity entity
    {
       get
        {
            return this.entity();
        }
    }

    private bool _addedUpdate = false;
    protected void AddUpdate(Action func, int index)
    {
        _addedUpdate = true;
        _updateService?.AddUpdate(this, func, index);
    }

    protected void AddTokenUpdate(Action<CancellationToken> func, int index)
    {
        _addedUpdate = true;
        _updateService?.AddTokenUpdate(this, func, index);
    }
	
    protected virtual void OnDestroy()
    {
        if (_addedUpdate)
        {
            _updateService?.RemoveUpdates(this);
        }
    }
}

