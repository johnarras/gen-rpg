using Assets.Scripts.Tokens;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Assets.Scripts.GameObjects;
using System.Threading.Tasks;

public class UpdateType
{
    public const int Regular = 0;
    public const int Late = 1;
    public const int Max = 2;
}

public interface IUpdateObject
{
    object Obj { get; set; }
    void Call(CancellationToken token);
    int UpdateType { get; set; }
    bool HasAction();
}

internal class VoidUpdateObject : IUpdateObject
{
    public object Obj { get; set; }
    public Action action { get; set; }
    public int UpdateType { get; set; }

    public bool HasAction()
    {
        return action != null;
    }

    public void Call(CancellationToken token)
    {
        if (!ObjectUtils.IsNull(Obj) && action != null)
        {
            action();
        }
    }
}

internal class TokenUpdateObject : IUpdateObject
{
    public object Obj { get; set; }
    public Action<CancellationToken> action { get; set; }
    public int UpdateType { get; set; }
    public bool HasAction()
    {
        return action != null;
    }

    public void Call(CancellationToken token)
    {
        if (!ObjectUtils.IsNull(Obj) && action != null)
        {
            action(token);
        }
    }
}


public interface IUnityUpdateService : IInitializable, IMapTokenService, IGameTokenService
{
    void AddUpdate(object objIn, Action funcIn, int updateType);
    void AddTokenUpdate(object objIn, Action<CancellationToken> funcIn, int updateType);
    void RemoveUpdates(object obj);
    void AddDelayedUpdate(object objIn, Action<CancellationToken> funcIn, CancellationToken token, float delaySeconds);
}

public class UnityUpdateService : StubComponent, IUnityUpdateService
{

    private List<IUpdateObject> _currentUpdates { get; set; } = new List<IUpdateObject>();
    private List<IUpdateObject> _toAddList { get; set; } = new List<IUpdateObject>();
    private List<object> _toRemoveList { get; set; } = new List<object>();


    private IUnityGameState _gs;
    public async Task Initialize(CancellationToken token)
    {
        _mapToken = token;
        await Task.CompletedTask;
        
    }

    private CancellationToken _gameToken;
    public void SetGameToken(CancellationToken token)
    {
        _gameToken = token;
    }

    private CancellationToken _mapToken;
    public void SetMapToken (CancellationToken token)
    {
        _mapToken = token;
    }

    private void Update ()
    {
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
        UpdateUpdates();
    }

    private void LateUpdate ()
    {
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
        UpdateUpdates();
    }

    public void AddUpdate(object objIn, Action funcIn, int updateType)
    {
        if (ObjectUtils.IsNull(objIn) || funcIn == null || updateType < UpdateType.Regular || updateType >= UpdateType.Max)
        {
            return;
        }

        _toAddList.Add(new VoidUpdateObject()
        {
            action = funcIn,
            Obj = objIn,
            UpdateType = updateType,
        });
    }

    public void AddTokenUpdate(object objIn, Action<CancellationToken> funcIn, int updateType)
    {
        if (ObjectUtils.IsNull(objIn) || funcIn == null || updateType < UpdateType.Regular || updateType >= UpdateType.Max)
        {
            return;
        }

        _toAddList.Add(new TokenUpdateObject()
        {
            action = funcIn,
            Obj = objIn,
            UpdateType = updateType,
        });
    }


    public void RemoveUpdates(object obj)
    {
        if (ObjectUtils.IsNull(obj))
        {
            return;
        }

        _toRemoveList.Add(obj);
        _toAddList = _toAddList.Where(x => x.Obj != obj).ToList();
    }

    private List<IUpdateObject> _addUpdates = new List<IUpdateObject>();
    private List<IUpdateObject> _removeUpdates = new List<IUpdateObject>();
    IUpdateObject _currObj = null;
    IUpdateObject _newUpdate = null;
    IUpdateObject _existingUpdate = null;
    private bool _isInRemoveList = false;
    private void UpdateUpdates()
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
            if (ObjectUtils.IsNull(_currObj.Obj) || !_currObj.HasAction())
            {
                _isInRemoveList = false;
                for (int r = 0; r < _toRemoveList.Count; r++)
                {
                    if (_toRemoveList[r] == _currObj.Obj)
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
                ObjectUtils.IsNull(_newUpdate.Obj) ||
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

        for (int r = 0;  r < _removeUpdates.Count; r++)
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
    public void AddDelayedUpdate(object objIn, Action<CancellationToken> funcIn, CancellationToken token, float delaySeconds)
    {
        _delayedUpdates.Add(new DelayedUpdate()
        {
            Obj = objIn,
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
            if (!ObjectUtils.IsNull(readyUpdate.Obj) && readyUpdate.Function != null && !readyUpdate.Token.IsCancellationRequested)
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