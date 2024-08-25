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

        public List<Reward> Roll<SI>(IRandom rand, RollData rollData, SI spawnItem) where SI : ISpawnItem
        {
            List<Reward> retval = new List<Reward>();

            ItemType itype = _gameData.Get<ItemTypeSettings>(null).Get(spawnItem.EntityId);

            if (itype == null)
            {
                return retval;
            }

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
                Reward sr = new Reward();
                sr.EntityId = spawnItem.EntityId;
                sr.EntityTypeId = EntityTypes.Item;
                sr.Quantity = 1;
                sr.QualityTypeId = rollData.QualityTypeId;
                sr.Level = rollData.Level;
                retval.Add(sr);

                sr.ExtraData = _itemGenService.Generate(rand, igd);
                sr.Quantity = rollData.QualityTypeId;
            }
            else
            {
                for (int i = 0; i < quantity; i++)
                {
                    Reward sr = new Reward();
                    sr.EntityId = spawnItem.EntityId;
                    sr.EntityTypeId = EntityTypes.Item;
                    sr.Quantity = 1;
                    sr.QualityTypeId = rollData.QualityTypeId;
                    sr.Level = rollData.Level;
                    retval.Add(sr);

                    sr.ExtraData = _itemGenService.Generate(rand, igd);
                }
            }
            return retval;
        }
    }
}
