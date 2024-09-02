using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.AuthCommandHandlers.Constants
{
    public enum EAuthResult
    {
        Failure = 0,
        UsedPassword = 1,
        UsedToken = 2,
    }
}
