using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Login.Messages.RefreshGameSettings;
using System.Threading;

namespace Assets.Scripts.Login.MessageHandlers
{
    public class RefreshGameSettingsResultHandler : BaseClientLoginResultHandler<RefreshGameSettingsResult>
    {
        protected override void InnerProcess(UnityGameState gs, RefreshGameSettingsResult result, CancellationToken token)
        {
            gs.ch.SetGameDataOverrideList(result.Overrides);         
            _gameData.AddData(result.NewSettings);
        }
    }
}
