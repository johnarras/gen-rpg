using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Requests.Entities
{
    public interface IRequestHandler : ISetupDictionaryItem<Type>
    {
        Task<IResponse> HandleRequest(ServerGameState gs, IRequest request, ResponseEnvelope envelope, CancellationToken token);
    }
}
