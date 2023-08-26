using Assets.Scripts.Login.Messages;
using Assets.Scripts.Login.Messages.Core;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Login.Messages.UploadMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Login.MessageHandlers.Core
{
    public class UploadMapResultHandler : BaseClientLoginResultHandler<UploadMapResult>
    {
        protected override void InnerProcess(UnityGameState gs, UploadMapResult result, CancellationToken token)
        {
        }
    }
}
