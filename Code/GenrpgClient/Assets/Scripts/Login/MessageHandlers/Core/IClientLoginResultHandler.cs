using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;

namespace Assets.Scripts.Login.Messages
{
    public interface IClientLoginResultHandler : ISetupDictionaryItem<Type>
    {

        int Priority();
        void Process(IWebResult result, CancellationToken token);
    }
}
