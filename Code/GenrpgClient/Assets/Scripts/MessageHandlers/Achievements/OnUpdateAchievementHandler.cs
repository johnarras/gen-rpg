using Genrpg.Shared.Achievements.Entities;
using Genrpg.Shared.Achievements.Messages;
using Genrpg.Shared.Currencies.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
