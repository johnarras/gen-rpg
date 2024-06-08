using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Login.Messages.RefreshGameSettings;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class RefreshGameSettingsResultHandler : BaseClientLoginResultHandler<RefreshGameSettingsResult>
    {
        protected override void InnerProcess(RefreshGameSettingsResult result, CancellationToken token)
        {
            _gs.ch.SetGameDataOverrideList(result.Overrides);         
            _gameData.AddData(result.NewSettings);
        }
    }
}
