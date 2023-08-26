using Assets.Scripts.Tokens;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class UpdateType
{
    public const int Regular = 0;
    public const int Late = 1;
    public const int Max = 2;
}

public interface IUpdateObject
{
    object Obj { get; set; }
    void Call(IUnityUpdateService unityUpdateService);
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

    public void Call(IUnityUpdateService unityUpdateService)
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

    public void Call(IUnityUpdateService unityUpdateService)
    {
        if (Obj != null && action != null && TokenUtils.IsValid(unityUpdateService.GetToken()))
        {
            action(unityUpdateService.GetToken());
        }
    }
}


public interface IUnityUpdateService : IService, IMapTokenService
{
    void AddUpdate(object objIn, Action funcIn, int updateType);
    void AddTokenUpdate(object objIn, Action<CancellationToken> funcIn, int updateType);
    void RemoveUpdates(object obj);
    CancellationToken GetToken();
}

public class UnityUpdateService : MonoBehaviour, IUnityUpdateService
{

    private List<IUpdateObject> UpdateObjects { get; set; }
    List<IUpdateObject> AddList { get; set; }
    List<object> RemoveList { get; set; }
    protected void Awake()
    {
        Reset();
    }

    public void Reset()
    {        
        UpdateObjects = new List<IUpdateObject>();
        AddList = new List<IUpdateObject>();
        RemoveList = new List<object>();
    }

    private CancellationToken _token;
    public void SetToken (CancellationToken token)
    {
        _token = token;
    }

    public CancellationToken GetToken()
    {
        return _token;
    }

    public void Update()
    {
        foreach (IUpdateObject obj in UpdateObjects)
        {
            if (obj.UpdateType == UpdateType.Regular)
            {
                obj.Call(this);
            }
        }
        UpdateUpdates();

    }

    public void LateUpdate()
    {
        foreach (IUpdateObject obj in UpdateObjects)
        {
            if (obj.UpdateType == UpdateType.Late)
            {
                obj.Call(this);
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

        AddList.Add(new VoidUpdateObject()
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

        AddList.Add(new TokenUpdateObject()
        {
            action = funcIn,
            Obj = objIn,
            UpdateType = updateType,
        });
    }


    public void RemoveUpdates(object obj)
    {
        RemoveList.Add(obj);
        //UpdateObjects = UpdateObjects.Where(x => x.Obj != obj).ToList();
        AddList = AddList.Where(x => x.Obj != obj).ToList();
    }

    private void UpdateUpdates()
    {
        UpdateObjects = UpdateObjects.Where(x => x.Obj != null  && x.HasAction() && !RemoveList.Contains(x.Obj)).ToList();

        foreach (IUpdateObject data in AddList)
        {
            if (!data.HasAction() || data.Obj == null || data.UpdateType < UpdateType.Regular || data.UpdateType >= UpdateType.Max)
            {
                continue;
            }
            List<IUpdateObject> currentUpdates = UpdateObjects.Where(x => x.Obj == data.Obj &&            
            x.UpdateType == data.UpdateType).ToList();

            foreach (IUpdateObject update in currentUpdates)
            {
                UpdateObjects.Remove(update);
            }

            UpdateObjects.Add(data);
        }
        RemoveList = new List<object>();
    }
}