using Genrpg.Shared.Achievements.Messages;
using Genrpg.Shared.Achievements.PlayerData;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Achievements
{
    public class OnUpdateAchievementHandler : BaseClientMapMessageHandler<OnUpdateAchievement>
    {
        protected override void InnerProcess(OnUpdateAchievement msg, CancellationToken token)
        {
            _gs.ch.Get<AchievementData>().Get(msg.AchievementTypeId).Quantity = msg.Quantity;
        }
    }
}
