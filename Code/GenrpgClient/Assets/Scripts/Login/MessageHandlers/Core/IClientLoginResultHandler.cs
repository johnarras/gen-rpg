using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using System;
using System.Threading;

namespace Assets.Scripts.Login.Messages
{
    public interface IClientLoginResultHandler : ISetupDictionaryItem<Type>
    {

        int Priority();
        void Process(UnityGameState gs, ILoginResult result, CancellationToken token);
    }
}
