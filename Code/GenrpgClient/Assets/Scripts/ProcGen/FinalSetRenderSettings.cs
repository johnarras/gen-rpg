

using Cysharp.Threading.Tasks;
using System.Threading;

public class SetFinalRenderSettings: BaseZoneGenerator
{
    public override async UniTask Generate (UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
    }
}
	
