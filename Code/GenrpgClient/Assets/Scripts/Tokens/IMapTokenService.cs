﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Tokens
{
    public interface IMapTokenService
    {
        void SetToken(CancellationToken token);
    }
}