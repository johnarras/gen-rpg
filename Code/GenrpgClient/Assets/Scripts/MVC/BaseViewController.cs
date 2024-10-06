using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MVC
{
    public class BaseViewController<TModel, IView> : IViewController<TModel, IView>
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
        protected IClientEntityService _gameObjectService;

        protected TModel _model;
        protected IView _view;
        protected CancellationToken _token;

        private CancellationTokenSource _cts;
        
        public virtual IView GetView()
        {
            return _view;
        }

        public virtual TModel GetModel()
        {
            return _model;
        }

        public virtual CancellationToken GetToken()
        {
            return _token;
        }

        public virtual async Task Init(TModel model, IView view, CancellationToken token)
        {
            _model = model; 
            _view = view;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _token = _cts.Token;
            await Task.CompletedTask;
        }

        protected void AddUpdate(Action func, int index)
        {
            _updateService?.AddUpdate(this, func, index, GetToken());
        }

        protected void AddTokenUpdate(Action<CancellationToken> func, int index)
        {
            _updateService?.AddTokenUpdate(this, func, index, GetToken());
        }

        protected void AddDelayedUpdate(Action<CancellationToken> func, float delaySeconds)
        {
            _updateService?.AddDelayedUpdate(this, func, delaySeconds, GetToken());
        }

        protected void AddListener<T>(GameAction<T> action) where T : class
        {
            _dispatcher.AddListener<T>(action, GetToken());
        }
    }
}
