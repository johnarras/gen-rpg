using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using UnityEngine;


public class Dispatcher : IDispatcher
{
    private IInitClient _initClient;

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    private Dictionary<Type, object> _dict = new Dictionary<Type, object>();

    public void AddListener<T>(GameAction<T> action, CancellationToken token) where T : class
    {
        token.Register(() => { RemoveListener(action); });

        if (!_dict.ContainsKey(typeof(T)))
        {
            _dict[typeof(T)] = new List<GameAction<T>>();
        }

        List<GameAction<T>> list = (List<GameAction<T>>)_dict[typeof(T)];
        if (!list.Contains(action))
        {
            list.Add(action);
        }
    }

    /// <summary>
    /// May need to make this public someday, but these events seem to stick around for the life of the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    private void RemoveListener<T>(GameAction<T> action) where T : class
    {
        if (!_dict.ContainsKey(typeof(T)))
        {
            return;
        }
        List<GameAction<T>> list = (List<GameAction<T>>)_dict[typeof(T)];
        if (list.Contains(action))
        {
            list.Remove(action);
        }
    }

    public void Dispatch<T>(T actionParam) where T : class
    {
        if (!_dict.ContainsKey(typeof(T)))
        {
            return;
        }

        List<GameAction<T>> list = new List<GameAction<T>>((List<GameAction<T>>)_dict[typeof(T)]);

        foreach (GameAction<T> gameAction in list)
        {
            gameAction(actionParam);
        }
    }
}
