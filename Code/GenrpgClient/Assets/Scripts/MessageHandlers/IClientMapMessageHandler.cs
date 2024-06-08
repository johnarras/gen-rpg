using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Threading;

public interface IClientMapMessageHandler : ISetupDictionaryItem<Type>
{
    // Purposely not Task because there is jankiness needing to return to main thread before doing
    // Unity actions.
    void Process(IMapApiMessage msg, CancellationToken token);
}
