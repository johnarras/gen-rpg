using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.Quests.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Messages;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Spawns.Entities;
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

        public override bool GiveRewards<SR>(IRandom rand, MapObject obj, List<SR> resultList)
        {
            foreach (ISpawnResult spawnResult in resultList)
            {
                _questService.UpdateQuest(rand, obj, spawnResult);
            }

            return base.GiveRewards(rand, obj, resultList);
        }

        public override void OnSetQuantity<TUpd>(Unit unit, TUpd upd, long entityTypeId, long diff)
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
                    CharId = unit.Id,
                    EntityTypeId = entityTypeId,
                    EntityId = quantityChild.IdKey,
                    Quantity = diff,
                };

                _messageService.SendMessage(unit, onAdd);
            }
        }
    }
}
