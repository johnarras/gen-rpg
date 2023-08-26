using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.MainServer
{
    public interface IServer
    {
        Task Init(object data, CancellationToken serverToken);
        Task Run();
    }
}
