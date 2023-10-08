using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;



public class InitTooltipData 
{
}


public abstract class BaseTooltip : BaseBehaviour
{
    protected CancellationToken _token;
    public virtual void Init(UnityGameState gs, InitTooltipData initData, CancellationToken token)
    {
        _token = token;
    }
}