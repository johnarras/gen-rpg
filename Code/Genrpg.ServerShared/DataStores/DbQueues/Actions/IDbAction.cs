using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.DbQueues.Actions
{
    public interface IDbAction
    {
        Task<bool> Execute();
    }
}
