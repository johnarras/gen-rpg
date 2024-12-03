using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Core.Interfaces
{
    public interface IGameCleanup
    {
        Task OnCleanup(CancellationToken token);
    }
}
