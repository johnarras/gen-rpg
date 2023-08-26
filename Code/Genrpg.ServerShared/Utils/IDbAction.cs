using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Utils
{
    public interface IDbAction
    {
        Task<bool> Execute();
    }
}
