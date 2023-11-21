using Genrpg.Shared.GameSettings.Messages;
using Genrpg.Shared.Login.Messages.RefreshGameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.GameSettings
{
    public class OnRefreshGameSettingsMessageHandler : BaseClientMapMessageHandler<OnRefreshGameSettings>
    {
        private INetworkService _networkService = null;

        protected override void InnerProcess(UnityGameState gs, OnRefreshGameSettings msg, CancellationToken token)
        {
            _networkService.SendClientWebCommand(new RefreshGameSettingsCommand() { CharId = gs.ch.Id }, token);
        }
    }
}
