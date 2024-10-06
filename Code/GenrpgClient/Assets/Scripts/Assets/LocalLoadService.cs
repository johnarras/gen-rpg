using Genrpg.Shared.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Assets
{
    public interface ILocalLoadService : IInjectable
    {
        T LocalLoad<T>(string path);
    }

    public class LocalLoadService : ILocalLoadService
    {
        public T LocalLoad<T>(string path)
        {
            object obj = Resources.Load(path, typeof(T));
            return (T)obj;
        }
    }
}
