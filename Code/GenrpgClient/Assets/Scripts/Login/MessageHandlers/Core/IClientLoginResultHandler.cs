using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Login.Messages
{
    public interface IClientLoginResultHandler : ISetupDictionaryItem<Type>
    {
        void Process(UnityGameState gs, ILoginResult result, CancellationToken token);
    }
}
