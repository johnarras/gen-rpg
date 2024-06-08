using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using System;
using System.Threading;

namespace Assets.Scripts.Login.Messages
{
    public interface IClientLoginResultHandler : ISetupDictionaryItem<Type>
    {

        int Priority();
        void Process(ILoginResult result, CancellationToken token);
    }
}
