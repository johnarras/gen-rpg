﻿using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Website.Messages.UploadMap;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers.Core
{
    public class UploadMapResultHandler : BaseClientLoginResultHandler<UploadMapResult>
    {
        protected override void InnerProcess(UploadMapResult result, CancellationToken token)
        {
        }
    }
}
