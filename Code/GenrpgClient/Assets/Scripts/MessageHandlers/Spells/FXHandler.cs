
using Genrpg.Shared.Spells.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class FXHandler : BaseClientMapMessageHandler<FX>
    {
        protected IFxService _fxService;
        protected override void InnerProcess(UnityGameState gs, FX msg, CancellationToken token)
        {
            _fxService.ShowFX(gs, msg, token);
        }
    }
}
