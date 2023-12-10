using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Tokens
{
    public interface IGameTokenService
    {
        void SetGameToken(CancellationToken token);
    }
}
