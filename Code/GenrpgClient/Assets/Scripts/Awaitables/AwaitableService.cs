using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Awaitables
{
    public interface IAwaitableService : IInjectable
    {
        void ForgetAwaitable(Awaitable t);
        void ForgetAwaitable<A>(Awaitable<A> t);
    }

    public class AwaitableService : IAwaitableService
    {
        public void ForgetAwaitable(Awaitable t)
        {
            _ = Task.Run(() => t);
        }

        public void ForgetAwaitable<A>(Awaitable<A> t)
        {
            _ = Task.Run(() => t);
        }
    }
}
