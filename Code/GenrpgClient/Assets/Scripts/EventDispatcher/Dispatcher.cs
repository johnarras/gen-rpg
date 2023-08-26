using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Should probably use 2 generic params, but that's more complicated for now.
public delegate T GameAction<T>(UnityGameState gs, T t);


public class Dispatcher
{
    private Dictionary<Type, object> _dict = new Dictionary<Type, object>();


    public void AddEvent<T>(GameAction<T> action) where T : class
    {
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

    public T DispatchEvent<T>(UnityGameState gs, T actionParam) where T : class
    {
        if (!_dict.ContainsKey(typeof(T)))
        {
            return default;
        }

        T retval = null;

        List<GameAction<T>> list = new List<GameAction<T>>((List<GameAction<T>>)_dict[typeof(T)]);

        foreach (GameAction<T> gameAction in list)
        {
            T tempVal = gameAction(gs, actionParam);
            if (tempVal != null && retval == null)
            {
                retval = tempVal;
            }
        }

        return retval;
    }


    public void Clear()
    {
        _dict = new Dictionary<Type, object>();
    }
}
