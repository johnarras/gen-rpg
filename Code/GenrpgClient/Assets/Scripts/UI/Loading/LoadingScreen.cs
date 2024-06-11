
using System.Threading;
using UnityEngine;


public class LoadingScreen : BaseScreen
{
    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
        // Play music null plays music track 1 and ambient track 0 (none)
        _audioService.PlayMusic(null);
        

    }
}

