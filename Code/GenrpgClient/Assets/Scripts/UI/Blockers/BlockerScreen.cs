
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BlockerScreen : BaseScreen
{
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await Task.CompletedTask;
    }
}
