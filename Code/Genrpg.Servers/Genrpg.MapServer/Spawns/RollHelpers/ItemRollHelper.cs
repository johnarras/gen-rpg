using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using Microsoft.Extensions.Azure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.RollHelpers
{
    public class ItemRollHelper : IRollHelper
    {
        public long GetKey() { return EntityTypes.Item; }

        private IItemGenService _itemGenService = null;
        private IGameData _gameData = null;

        public List<RewardList> Roll<SI>(IRandom rand, RollData rollData, SI spawnItem) where SI : ISpawnItem
        {
            List<RewardList> retval = new List<RewardList>();

            ItemType itype = _gameData.Get<ItemTypeSettings>(null).Get(spawnItem.EntityId);

            if (itype == null)
            {
                return retval;
            }

            RewardList rewardList = new RewardList();
            retval.Add(rewardList);
            long quantity = MathUtils.LongRange(spawnItem.MinQuantity, spawnItem.MaxQuantity, rand);

            ItemGenData igd = new ItemGenData()
            {
                ItemTypeId = spawnItem.EntityId,
                Level = rollData.Level,
                QualityTypeId = rollData.QualityTypeId,
                Quantity = 1,
            };

            if (itype.CanStack())
            {
                Reward rew = new Reward();
                rew.EntityId = spawnItem.EntityId;
                rew.EntityTypeId = EntityTypes.Item;
                rew.Quantity = 1;
                rew.QualityTypeId = rollData.QualityTypeId;
                rew.Level = rollData.Level;
                rewardList.Rewards.Add(rew);

                rew.ExtraData = _itemGenService.Generate(rand, igd);
                rew.Quantity = rollData.QualityTypeId;
            }
            else
            {
                for (int i = 0; i < quantity; i++)
                {
                    Reward rew = new Reward();
                    rew.EntityId = spawnItem.EntityId;
                    rew.EntityTypeId = EntityTypes.Item;
                    rew.Quantity = 1;
                    rew.QualityTypeId = rollData.QualityTypeId;
                    rew.Level = rollData.Level;
                    rewardList.Rewards.Add(rew);

                    rew.ExtraData = _itemGenService.Generate(rand, igd);
                }
            }
            return retval;
        }
    }
}
