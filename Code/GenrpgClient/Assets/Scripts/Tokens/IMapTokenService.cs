using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace Assets.Scripts.Tokens
{
    public interface IMapTokenService
    {
        void SetMapToken(CancellationToken token);
    }
}
