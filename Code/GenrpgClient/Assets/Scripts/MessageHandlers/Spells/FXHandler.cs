
using Genrpg.Shared.Spells.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class FXHandler : BaseClientMapMessageHandler<FX>
    {
        protected IAssetService _assetService;
        protected override void InnerProcess(UnityGameState gs, FX msg, CancellationToken token)
        {
            FXManager.ShowFX(gs, msg, _objectManager, _assetService, token);
        }
    }
}
