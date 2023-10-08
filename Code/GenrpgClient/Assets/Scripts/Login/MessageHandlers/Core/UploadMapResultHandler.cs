using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Login.Messages.UploadMap;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers.Core
{
    public class UploadMapResultHandler : BaseClientLoginResultHandler<UploadMapResult>
    {
        protected override void InnerProcess(UnityGameState gs, UploadMapResult result, CancellationToken token)
        {
        }
    }
}
