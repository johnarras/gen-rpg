using Genrpg.Shared.GameSettings.Messages;
using Genrpg.Shared.Website.Messages.RefreshGameSettings;
using Genrpg.Shared.Website.Messages.RefreshStores;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.GameSettings
{
    public class OnRefreshGameSettingsMessageHandler : BaseClientMapMessageHandler<OnRefreshGameSettings>
    {
        private IClientWebService _webNetworkService = null;

        protected override void InnerProcess(OnRefreshGameSettings msg, CancellationToken token)
        {
            _webNetworkService.SendClientWebCommand(new RefreshGameSettingsCommand() { CharId = _gs.ch.Id }, token);
            _webNetworkService.SendClientWebCommand(new RefreshStoresCommand() { CharId = _gs.ch.Id }, token);
        }
    }
}
