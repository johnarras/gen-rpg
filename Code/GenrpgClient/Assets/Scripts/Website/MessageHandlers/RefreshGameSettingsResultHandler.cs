using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.GameSettings.WebApi.RefreshGameSettings;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class RefreshGameSettingsResultHandler : BaseClientLoginResultHandler<RefreshGameSettingsResponse>
    {
        protected override void InnerProcess(RefreshGameSettingsResponse result, CancellationToken token)
        {
            _gs.ch.DataOverrides = result.DataOverrides;
            _gameData.AddData(result.NewSettings);
        }
    }
}
