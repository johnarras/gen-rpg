using Cysharp.Threading.Tasks;
using Genrpg.Shared.Login.Interfaces;
using System;
using System.Threading;

namespace Assets.Scripts.Login.Messages.Core
{
    public abstract class BaseClientLoginResultHandler<T> : IClientLoginResultHandler where T : class, ILoginResult
    {
        public Type GetKey() { return typeof(T); }

        virtual public int Priority() { return 0; }

        protected abstract void InnerProcess(UnityGameState gs, T result, CancellationToken token);

        public void Process(UnityGameState gs, ILoginResult result, CancellationToken token)
        {
            InnerProcess(gs, result as T, token);
        }
    }
}
