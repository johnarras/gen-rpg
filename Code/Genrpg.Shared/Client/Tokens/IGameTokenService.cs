using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Genrpg.Shared.Client.Tokens
{
    public interface IGameTokenService
    {
        void SetGameToken(CancellationToken token);
    }
}
