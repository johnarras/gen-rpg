
using System.Threading;
using System.Threading.Tasks;

public class LoadingScreen : BaseScreen
{
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        // Play music null plays music track 1 and ambient track 0 (none)
        _audioService.PlayMusic(_gs, null);
        await Task.CompletedTask;

    }
}

