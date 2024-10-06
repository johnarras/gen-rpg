
using Genrpg.Shared.Interfaces;
using System.Threading;

namespace Genrpg.Shared.Client.Core
{

    public delegate void GameAction<T>(T t);

    public interface IDispatcher : IInitializable
    {
        void AddListener<T>(GameAction<T> action, CancellationToken token) where T : class;
        void Dispatch<T>(T actionParam) where T : class;

    }
}
