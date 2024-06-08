
using Cysharp.Threading.Tasks;
using UI.Screens.Constants;
using System.Threading;

public class AfterGenerateMap : BaseZoneGenerator
{

    protected IScreenService _screenService;
    public override async UniTask Generate (CancellationToken token)
    {
        await base.Generate(token);

        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
        {
            _screenService.CloseAll();
            _screenService.Open(ScreenId.CharacterSelect);
        }
        await UniTask.CompletedTask;
	}
	

}
	
