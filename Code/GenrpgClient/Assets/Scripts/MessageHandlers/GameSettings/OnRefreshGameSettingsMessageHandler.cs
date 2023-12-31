using Genrpg.Shared.GameSettings.Messages;
using Genrpg.Shared.Login.Messages.RefreshGameSettings;
using Genrpg.Shared.Login.Messages.RefreshStores;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.GameSettings
{
    public class OnRefreshGameSettingsMessageHandler : BaseClientMapMessageHandler<OnRefreshGameSettings>
    {
        private IWebNetworkService _webNetworkService = null;

        protected override void InnerProcess(UnityGameState gs, OnRefreshGameSettings msg, CancellationToken token)
        {
            _webNetworkService.SendClientWebCommand(new RefreshGameSettingsCommand() { CharId = gs.ch.Id }, token);
            _webNetworkService.SendClientWebCommand(new RefreshStoresCommand() { CharId = gs.ch.Id }, token);
        }
    }
}
