﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Units.Entities;

public class InitTooltipData 
{
}


public abstract class BaseTooltip : BaseBehaviour
{
    protected CancellationToken _token;
    public virtual void Init(InitTooltipData initData, CancellationToken token)
    {
        _token = token;
    }
}