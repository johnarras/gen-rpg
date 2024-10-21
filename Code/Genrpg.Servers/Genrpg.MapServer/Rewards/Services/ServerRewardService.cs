using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.Quests.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Messages;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Rewards.Services
{
    public class ServerRewardService : RewardService
    {
        IServerQuestService _questService = null;
        protected IMapMessageService _messageService = null;

        public override bool GiveRewards<RL>(IRandom rand, MapObject obj, List<RL> resultList)
        {
            foreach (RewardList rl in resultList)
            {
                foreach (Reward reward in rl.Rewards)
                {
                    _questService.UpdateQuest(rand, obj, reward);
                }
            }

            return base.GiveRewards(rand, obj, resultList);
        }

        public override void OnAddQuantity<TUpd>(MapObject obj, TUpd upd, long entityTypeId, long entityId, long diff)
        {
            if (diff == 0)
            {
                return;
            }
            _repoService.QueueSave(upd);

            if (upd is IOwnerQuantityChild quantityChild)
            {
                OnAddQuantityReward onAdd = new OnAddQuantityReward()
                {
                    CharId = obj.Id,
                    EntityTypeId = entityTypeId,
                    EntityId = entityId,
                    Quantity = diff,
                };

                _messageService.SendMessage(obj, onAdd);
            }
        }
    }
}
