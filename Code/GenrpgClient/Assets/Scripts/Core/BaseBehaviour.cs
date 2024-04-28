using GEntity = UnityEngine.GameObject;
using System;

using Assets.Scripts.Interfaces;
using System.Threading;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.GameSettings;

public class BaseBehaviour : StubComponent
{
    protected IAudioService _audioService;
    protected IUnityUpdateService _updateService;
    protected IScreenService _screenService;
    protected IRealtimeNetworkService _networkService;
    protected IAssetService _assetService;
    protected IUIInitializable _uIInitializable;
    protected ILogService _logService;
    protected IDispatcher _dispatcher;
    protected IGameData _gameData;

    public UnityGameState _gs { get; set; }
    
    public virtual void Initialize(UnityGameState gs)
    {
        _gs = gs;
        _gs.loc.Resolve(this);
    }

    public CancellationToken GetToken()
    {
        return this.GetCancellationToken();
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

