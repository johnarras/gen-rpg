using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public interface IClientMapMessageHandler : ISetupDictionaryItem<Type>
{
    // Purposely not UniTask because there is jankiness needing to return to main thread before doing
    // Unity actions.
    void Process(UnityGameState gs, IMapApiMessage msg, CancellationToken token);
}
