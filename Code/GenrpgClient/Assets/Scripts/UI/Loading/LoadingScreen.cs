
using System.Threading;
using Cysharp.Threading.Tasks;

public class LoadingScreen : BaseScreen
{
    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        // Play music null plays music track 1 and ambient track 0 (none)
        _audioService.PlayMusic(null);
        await UniTask.CompletedTask;

    }
}

