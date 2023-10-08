using Assets.Scripts.Tokens;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Genrpg.Shared.Spells.Entities;

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
        if (Obj != null && action != null)
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
        if (Obj != null && action != null)
        {
            action(token);
        }
    }
}


public interface IUnityUpdateService : ISetupService, IMapTokenService
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


    private UnityGameState _gs;
    public async Task Setup(GameState gs, CancellationToken token)
    {
        _gs = gs as UnityGameState;
        _token = token;
        await Task.CompletedTask;
    }

    private CancellationToken _token;
    public void SetToken (CancellationToken token)
    {
        _token = token;
    }

    private void Update ()
    {
        foreach (IUpdateObject obj in _currentUpdates)
        {
            if (obj.UpdateType == UpdateType.Regular)
            {
                obj.Call(_token);
            }
        }
        UpdateUpdates();
    }

    private void LateUpdate ()
    {
        foreach (IUpdateObject obj in _currentUpdates)
        {
            if (obj.UpdateType == UpdateType.Late)
            {
                obj.Call(_token);
            }
        }
        UpdateUpdates();
    }

    public void AddUpdate(object objIn, Action funcIn, int updateType)
    {
        if (objIn == null || funcIn == null || updateType < UpdateType.Regular || updateType >= UpdateType.Max)
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
        if (objIn == null || funcIn == null || updateType < UpdateType.Regular || updateType >= UpdateType.Max)
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
        _toRemoveList.Add(obj);
        _toAddList = _toAddList.Where(x => x.Obj != obj).ToList();
    }

    List<IUpdateObject> nextUpdates = new List<IUpdateObject>();
    private void UpdateUpdates()
    {
        nextUpdates = new List<IUpdateObject>();
        foreach (IUpdateObject obj in _currentUpdates)
        {
            if (obj.Obj == null || !obj.HasAction() || _toRemoveList.Contains(obj.Obj))
            {
                continue;
            }
            nextUpdates.Add(obj);
        }
        

        foreach (IUpdateObject newUpdate in _toAddList)
        {
            if (!newUpdate.HasAction() || newUpdate.Obj == null || newUpdate.UpdateType < UpdateType.Regular || newUpdate.UpdateType >= UpdateType.Max)
            {
                continue;
            }

            IUpdateObject currentUpdate = nextUpdates.FirstOrDefault(x => x.Obj == newUpdate.Obj && x.UpdateType == newUpdate.UpdateType);

            if (currentUpdate == null)
            {
                nextUpdates.Add(newUpdate);
            }
        }

        UpdateDelayedActions();

        _currentUpdates = nextUpdates;
        _toRemoveList = new List<object>();
        _toAddList = new List<IUpdateObject>();

    }

    internal class DelayedUpdate
    {
        public Object Obj;
        public CancellationToken Token;
        public DateTime EndTime;
        public Action<CancellationToken> Function;
    }

    private List<DelayedUpdate> _delayedUpdates = new List<DelayedUpdate>();

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

    void UpdateDelayedActions()
    {
        List<DelayedUpdate> currUpdates = _delayedUpdates.Where(x => x.EndTime <= DateTime.UtcNow).ToList();

        if (currUpdates.Count == 0)
        {
            return;
        }

        _delayedUpdates = _delayedUpdates.Except(currUpdates).ToList();

        foreach (DelayedUpdate update in currUpdates)
        {
            if (update.Obj != null && update.Function != null && !update.Token.IsCancellationRequested)
            {
                update.Function.Invoke(update.Token);
            }
        }

    }
}