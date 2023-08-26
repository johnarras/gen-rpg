using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Messages
{
    public interface ICloudMessageHandler : ISetupDictionaryItem<Type> 
    {
        Task HandleMessage(ServerGameState gs, ICloudMessage message, CancellationToken token);
    }
}
