

using UI.Screens.Constants;
using System.Threading;
using UnityEngine;

public class AfterGenerateMap : BaseZoneGenerator
{

    protected IScreenService _screenService;
    public override async Awaitable Generate (CancellationToken token)
    {
        await base.Generate(token);

        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
        {
            _screenService.CloseAll();
            _screenService.Open(ScreenId.CharacterSelect);
        }
        
	}
	

}
	
