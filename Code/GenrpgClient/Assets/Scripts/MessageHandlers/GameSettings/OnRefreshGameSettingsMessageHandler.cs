using Genrpg.Shared.GameSettings.Messages;
using Genrpg.Shared.GameSettings.WebApi.RefreshGameSettings;
using Genrpg.Shared.Purchasing.WebApi.RefreshStores;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.GameSettings
{
    public class OnRefreshGameSettingsMessageHandler : BaseClientMapMessageHandler<OnRefreshGameSettings>
    {
        private IClientWebService _webNetworkService = null;

        protected override void InnerProcess(OnRefreshGameSettings msg, CancellationToken token)
        {
            _webNetworkService.SendClientUserWebRequest(new RefreshGameSettingsRequest() { CharId = _gs.ch.Id }, token);
            _webNetworkService.SendClientUserWebRequest(new RefreshStoresRequest() { CharId = _gs.ch.Id }, token);
        }
    }
}
