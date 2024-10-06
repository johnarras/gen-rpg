using Genrpg.Shared.Client.Updates;
using Genrpg.Shared.Interfaces;
using System.Threading;

namespace Genrpg.Shared.Client.Core
{
    public interface IInitClient : IInjectable
    {
        object go { get; }
        CancellationToken GetGameToken();
        void SetGlobalUpdater(IGlobalUpdater updater);
    }
}
