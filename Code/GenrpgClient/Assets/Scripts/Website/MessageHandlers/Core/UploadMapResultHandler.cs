using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.MapServer.WebApi.UploadMap;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers.Core
{
    public class UploadMapResultHandler : BaseClientLoginResultHandler<UploadMapResponse>
    {
        protected override void InnerProcess(UploadMapResponse result, CancellationToken token)
        {
        }
    }
}
