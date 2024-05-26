using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;

// Should probably use 2 generic params, but that's more complicated for now.
public delegate void GameAction<T>(T t);


public interface IDispatcher : IInitializable
{
    void AddEvent<T>(UnityEngine.MonoBehaviour monoBehaviour, GameAction<T> action) where T : class;
    void RemoveEvent<T>(GameAction<T> action) where T : class;
    void Dispatch<T>(T actionParam) where T : class;

}

public class Dispatcher : IDispatcher
{
    public async Task Initialize(GameState gs, CancellationToken token)
    {
        await Task.CompletedTask;
    }

    private Dictionary<Type, object> _dict = new Dictionary<Type, object>();

    public void AddEvent<T>(UnityEngine.MonoBehaviour monoBehaviour, GameAction<T> action) where T : class
    {
        monoBehaviour.GetCancellationToken().Register(() => { RemoveEvent(action); });
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

    public void RemoveEvent<T>(GameAction<T> action) where T : class
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
