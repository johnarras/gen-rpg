
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Client.Core;

public class UpdateType
{
    public const int Regular = 0;
    public const int Late = 1;
    public const int Max = 2;
}

public interface IUpdateObject
{
    object Obj { get; set; }
    CancellationToken Token { get; set; }
    void Call(CancellationToken token);
    int UpdateType { get; set; }
    bool HasAction();
}

internal class VoidUpdateObject : IUpdateObject
{
    public object Obj { get; set; }
    public CancellationToken Token { get; set; }
    public Action action { get; set; }
    public int UpdateType { get; set; }

    public bool HasAction()
    {
        return action != null;
    }

    public void Call(CancellationToken token)
    {
        if (!Token.IsCancellationRequested && action != null)
        {
            action();
        }
    }
}

internal class TokenUpdateObject : IUpdateObject
{
    public object Obj { get; set; }
    public CancellationToken Token { get; set; }
    public Action<CancellationToken> action { get; set; }
    public int UpdateType { get; set; }
    public bool HasAction()
    {
        return action != null;
    }

    public void Call(CancellationToken token)
    {
        if (!Token.IsCancellationRequested && action != null)
        {
            action(token);
        }
    }
}

public class ClientUpdateService : IClientUpdateService
{

    private List<IUpdateObject> _currentUpdates { get; set; } = new List<IUpdateObject>();
    private List<IUpdateObject> _toAddList { get; set; } = new List<IUpdateObject>();
    private List<IUpdateObject> _toRemoveList { get; set; } = new List<IUpdateObject>();
    private ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();

    private IClientGameState _gs;
    private IInitClient _initClient;
    public async Task Initialize(CancellationToken token)
    {
        _mapToken = token;
        _initClient.SetGlobalUpdater(this);
        await Task.CompletedTask;
    }

    private CancellationToken _gameToken;
    public void SetGameToken(CancellationToken token)
    {
        _gameToken = token;
    }

    private CancellationToken _mapToken;
    public void SetMapToken(CancellationToken token)
    {
        _mapToken = token;
    }

    public void RunOnMainThread(Action funcIn)
    {
        _mainThreadActions.Enqueue(funcIn); 
    }

    public void OnUpdate()
    {
        UpdateAfterUpdates();
        // Top level update in the game is outside of the task system
        if (_gameToken != CancellationToken.None && _gameToken.IsCancellationRequested)
        {
            return;
        }
        for (int c = 0; c < _currentUpdates.Count; c++)
        {
            if (_currentUpdates[c].UpdateType == UpdateType.Regular)
            {
                _currentUpdates[c].Call(_mapToken);
            }
        }
        UpdateAfterUpdates();
    }

    public void OnLateUpdate()
    {
        UpdateBeforeUpdates();
        // Top level update in the game is outside of the task system
        if (_gameToken != CancellationToken.None && _gameToken.IsCancellationRequested)
        {
            return;
        }
        for (int c = 0; c < _currentUpdates.Count; c++)
        {
            if (_currentUpdates[c].UpdateType == UpdateType.Late)
            {
                _currentUpdates[c].Call(_mapToken);
            }
        }
        UpdateAfterUpdates();
    }

    public void AddUpdate(object obj, Action funcIn, int updateType, CancellationToken token)
    {
        if (obj == null || token.IsCancellationRequested || funcIn == null || updateType < UpdateType.Regular || updateType >= UpdateType.Max)
        {
            return;
        }

        _toAddList.Add(new VoidUpdateObject()
        {
            Obj = obj,
            action = funcIn,
            Token = token,
            UpdateType = updateType,
        });
    }

    public void AddTokenUpdate(object obj, Action<CancellationToken> funcIn, int updateType, CancellationToken token)
    {
        if (obj == null || token.IsCancellationRequested || funcIn == null || updateType < UpdateType.Regular || updateType >= UpdateType.Max)
        {
            return;
        }

        _toAddList.Add(new TokenUpdateObject()
        {
            Obj = obj,
            action = funcIn,
            Token = token,
            UpdateType = updateType,
        });
    }

    private void UpdateBeforeUpdates()
    {
        while (_mainThreadActions.TryDequeue(out Action action))
        {
            action();
        }
    }

    private List<IUpdateObject> _addUpdates = new List<IUpdateObject>();
    private List<IUpdateObject> _removeUpdates = new List<IUpdateObject>();
    IUpdateObject _currObj = null;
    IUpdateObject _newUpdate = null;
    IUpdateObject _existingUpdate = null;
    private bool _isInRemoveList = false;
    private void UpdateAfterUpdates()
    {
        


        if (_addUpdates.Count > 0)
        {
            _addUpdates.Clear();
        }

        if (_removeUpdates.Count > 0)
        {
            _removeUpdates.Clear();
        }

        for (int c = 0; c < _currentUpdates.Count; c++)
        {
            _currObj = _currentUpdates[c];
            if (_currObj.Token.IsCancellationRequested || !_currObj.HasAction())
            {
                _isInRemoveList = false;
                for (int r = 0; r < _toRemoveList.Count; r++)
                {
                    if (_toRemoveList[r] == _currObj)
                    {
                        _isInRemoveList = true;
                        break;
                    }
                }

                if (!_isInRemoveList)
                {
                    _removeUpdates.Add(_currObj);
                }
            }
        }

        for (int a = 0; a < _toAddList.Count; a++)
        {
            _newUpdate = _toAddList[a];
            if (!_newUpdate.HasAction() ||
                _newUpdate.Token.IsCancellationRequested ||
                 _newUpdate.UpdateType < UpdateType.Regular || _newUpdate.UpdateType >= UpdateType.Max)
            {
                continue;
            }


            _existingUpdate = null;
            for (int b = 0; b < _addUpdates.Count; b++)
            {
                if (_addUpdates[b].Obj == _newUpdate.Obj && _addUpdates[b].UpdateType == _newUpdate.UpdateType)
                {
                    _existingUpdate = _addUpdates[b];
                    break;
                }
            }

            if (_existingUpdate == null)
            {
                _addUpdates.Add(_newUpdate);
            }
        }


        UpdateDelayedActions();

        for (int r = 0; r < _removeUpdates.Count; r++)
        {
            _currentUpdates.Remove(_removeUpdates[r]);
        }

        for (int a = 0; a < _addUpdates.Count; a++)
        {
            _currentUpdates.Add(_addUpdates[a]);
        }

        if (_toRemoveList.Count > 0)
        {
            _toRemoveList.Clear();
        }

        if (_toAddList.Count > 0)
        {
            _toAddList.Clear();
        }
    }

    internal class DelayedUpdate
    {
        public Object Obj;
        public CancellationToken Token;
        public DateTime EndTime;
        public Action<CancellationToken> Function;
    }

    private List<DelayedUpdate> _delayedUpdates = new List<DelayedUpdate>();

    DelayedUpdate readyUpdate = null;
    public void AddDelayedUpdate(object obj, Action<CancellationToken> funcIn, float delaySeconds, CancellationToken token)
    {
        _delayedUpdates.Add(new DelayedUpdate()
        {
            Obj = obj,
            Function = funcIn,
            Token = token,
            EndTime = DateTime.UtcNow.AddSeconds(delaySeconds),
        });
    }

    List<DelayedUpdate> readyUpdates = new List<DelayedUpdate>();
    void UpdateDelayedActions()
    {

        readyUpdates.Clear();
        for (int i = 0; i < _delayedUpdates.Count; i++)
        {
            if (_delayedUpdates[i].EndTime <= DateTime.UtcNow)
            {
                readyUpdates.Add(_delayedUpdates[i]);
            }
        }

        if (readyUpdates.Count == 0)
        {
            return;
        }

        for (int r = 0; r < readyUpdates.Count; r++)
        {
            readyUpdate = readyUpdates[r];
            if (readyUpdate.Function != null && !readyUpdate.Token.IsCancellationRequested)
            {
                readyUpdate.Function(readyUpdate.Token);
            }
        }

        for (int r = 0; r < readyUpdates.Count; r++)
        {
            _delayedUpdates.Remove(readyUpdates[r]);
        }

    }

}