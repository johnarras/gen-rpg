using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Queues.Entities
{
    public interface IQueueMessageHandler : ISetupDictionaryItem<Type>
    {
        Task HandleMessage(IQueueMessage message, CancellationToken token);
    }
}
