using System;
using System.Threading;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Interfaces;
using UnityEngine;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Client.Assets.Services;

public class BaseBehaviour : StubComponent, IInitOnResolve
{
    protected IInitClient _initClient;
    protected IClientUpdateService _updateService;
    protected IScreenService _screenService;
    protected IRealtimeNetworkService _networkService;
    protected IAssetService _assetService;
    protected IUIService _uiService;
    protected ILogService _logService;
    protected IDispatcher _dispatcher;
    protected IGameData _gameData;
    protected IClientGameState _gs;
    protected IClientRandom _rand;
    protected IClientEntityService _clientEntityService;

    private CancellationTokenSource _cts = null;
    public CancellationToken GetToken()
    {
        if (_cts == null)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(_initClient.GetGameToken(), this.destroyCancellationToken);
        }
        return _cts.Token;
    }

    

    private void ClearToken()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

    }

    public virtual void Init()
    {

    }
    public GameObject entity
    {
       get
        {
            return this.gameObject;
        }
    }

    protected void AddUpdate(Action func, int index)
    {
        _updateService?.AddUpdate(this, func, index, GetToken());
    }

    protected void AddTokenUpdate(Action<CancellationToken> func, int index)
    {
        _updateService?.AddTokenUpdate(this,func, index, GetToken());
    }

    protected void AddDelayedUpdate(Action<CancellationToken> func, float delaySeconds)
    {
        _updateService?.AddDelayedUpdate(this, func, delaySeconds, GetToken());
    }

    protected void AddListener<T>(GameAction<T> action) where T : class
    {
        _dispatcher.AddListener<T>(action, GetToken());
    }
	
    protected virtual void OnDestroy()
    {
        ClearToken();
    }
}

