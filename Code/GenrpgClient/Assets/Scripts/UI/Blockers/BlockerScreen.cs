using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class BlockerScreen : BaseScreen
{
    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await UniTask.CompletedTask;
    }
}
