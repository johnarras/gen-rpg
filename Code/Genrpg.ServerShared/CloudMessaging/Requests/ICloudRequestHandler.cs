using Genrpg.ServerShared.CloudMessaging.Messages;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Requests
{
    public interface ICloudRequestHandler : ISetupDictionaryItem<Type> 
    {
        Task<ICloudResponse> HandleRequest(ServerGameState gs, ICloudRequest request, CancellationToken token);
    }
}
