﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


public class TokenUtils
{
    public static bool IsValid(CancellationToken token)
    {
        return token != CancellationToken.None && !token.IsCancellationRequested;
    }
}
