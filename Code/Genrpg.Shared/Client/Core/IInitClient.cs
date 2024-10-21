using Genrpg.Shared.Client.Updates;
using Genrpg.Shared.Interfaces;
using System.Threading;

namespace Genrpg.Shared.Client.Core
{
    public interface IInitClient : IInjectable
    {
        CancellationToken GetGameToken();
        void SetGlobalUpdater(IGlobalUpdater updater);
    }
}
