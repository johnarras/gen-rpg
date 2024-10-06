
using Assets.Scripts.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class LoadingScreen : BaseScreen
{
    protected IAudioService _audioService;
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        // Play music null plays music track 1 and ambient track 0 (none)
        _audioService.PlayMusic(null);


        await Task.CompletedTask;
    }
}

