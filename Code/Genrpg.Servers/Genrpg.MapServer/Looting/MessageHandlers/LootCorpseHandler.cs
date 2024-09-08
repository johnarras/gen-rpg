using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Loot.Messages;
using Genrpg.ServerShared.Achievements;
using System.Linq;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.MapServer.Looting.MessageHandlers
{
    public class LootCorpseHandler : BaseCharacterServerMapMessageHandler<LootCorpse>
    {

        private IAchievementService _achievementService = null;

        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, LootCorpse message)
        {
            if (!_objectManager.GetUnit(message.UnitId, out Unit unit))
            {
                pack.SendError(ch, "That can't be looted");
                return;
            }

            if (!UnitUtils.AttackerInfoMatchesObject(unit.GetFirstAttacker(),ch))
            {
                pack.SendError(ch, "You can't loot that!");
                return;
            }


            List<RewardList> loot = new List<RewardList>();
            lock (unit.OnActionLock)
            {
                if (unit.Loot == null || unit.Loot.Count < 1)
                {
                    pack.SendError(ch, "That has no loot");
                    return;
                }
                loot = unit.Loot;
                unit.Loot = null;
            }

            long moneyTotal = 0;
            long itemTotal = 0;
            foreach (RewardList rewardList in loot)
            {
                moneyTotal += rewardList.Rewards.Where(x => x.EntityTypeId == EntityTypes.Currency && x.EntityId == CurrencyTypes.Money).Sum(x => x.Quantity);
                itemTotal += rewardList.Rewards.Where(x => x.EntityTypeId == EntityTypes.Item && x.ExtraData != null).Sum(x => x.Quantity);
            }

            _achievementService.UpdateAchievement(ch, AchievementTypes.MoneyLooted, moneyTotal);
            _achievementService.UpdateAchievement(ch,AchievementTypes.ItemsLooted, itemTotal);

            _rewardService.GiveRewards(rand, ch, loot);
            SendRewards sendLoot = new SendRewards()
            {
                ShowPopup = true,
                Rewards = loot,
            };
            ch.AddMessage(sendLoot);

            _messageService.SendMessageNear(unit, new ClearLoot() { UnitId = unit.Id });
        }
    }
}
