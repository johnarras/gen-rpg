using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Website.Messages.RefreshGameSettings;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class RefreshGameSettingsResultHandler : BaseClientLoginResultHandler<RefreshGameSettingsResult>
    {
        protected override void InnerProcess(RefreshGameSettingsResult result, CancellationToken token)
        {
            _gs.ch.DataOverrides = result.DataOverrides;
            _gameData.AddData(result.NewSettings);
        }
    }
}
