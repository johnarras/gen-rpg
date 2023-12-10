using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Login.Messages.RefreshGameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class RefreshGameSettingsResultHandler : BaseClientLoginResultHandler<RefreshGameSettingsResult>
    {
        protected override void InnerProcess(UnityGameState gs, RefreshGameSettingsResult result, CancellationToken token)
        {
            gs.ch.SetGameDataOverrideList(result.Overrides);
            gs.data.AddData(result.NewSettings);
        }
    }
}
