using Assets.Scripts.Login.Messages.Core;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using Services.ProcGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class LoadIntoMapResultHandler : BaseClientLoginResultHandler<LoadIntoMapResult>
    {
        private IZoneGenService _zoneGenService;
        protected override void InnerProcess(UnityGameState gs, LoadIntoMapResult result, CancellationToken token)
        {
            _zoneGenService.OnLoadIntoMap(gs, result, token).Forget();
        }
    }
}
