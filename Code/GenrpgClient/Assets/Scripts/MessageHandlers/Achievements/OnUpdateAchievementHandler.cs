using Genrpg.Shared.Achievements.Messages;
using Genrpg.Shared.Achievements.PlayerData;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Achievements
{
    public class OnUpdateAchievementHandler : BaseClientMapMessageHandler<OnUpdateAchievement>
    {
        protected override void InnerProcess(UnityGameState gs, OnUpdateAchievement msg, CancellationToken token)
        {
            gs.ch.Get<AchievementData>().Get(msg.AchievementTypeId).Quantity = msg.Quantity;
        }
    }
}
