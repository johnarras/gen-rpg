using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Entities
{
    public interface IPubSubMessageHandler : ISetupDictionaryItem<Type>
    {
        Task HandleMessage(IPubSubMessage message, CancellationToken token);
    }
}
