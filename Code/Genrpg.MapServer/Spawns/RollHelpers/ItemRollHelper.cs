using Genrpg.MapServer.Items;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
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

        public List<SpawnResult> Roll(GameState gs, RollData rollData, SpawnItem spawnItem)
        {
            List<SpawnResult> retval = new List<SpawnResult>();

            ItemType itype = gs.data.GetGameData<ItemTypeSettings>(null).GetItemType(spawnItem.EntityId);

            if (itype == null)
            {
                return retval;
            }

            long quantity = MathUtils.LongRange(spawnItem.MinQuantity, spawnItem.MaxQuantity, gs.rand);

            ItemGenData igd = new ItemGenData()
            {
                ItemTypeId = spawnItem.EntityId,
                Level = rollData.Level,
                QualityTypeId = rollData.QualityTypeId,
                Quantity = 1,
            };

            if (itype.CanStack())
            {
                SpawnResult sr = new SpawnResult();
                sr.EntityId = spawnItem.EntityId;
                sr.EntityTypeId = EntityTypes.Item;
                sr.Quantity = 1;
                sr.QualityTypeId = rollData.QualityTypeId;
                sr.Level = rollData.Level;
                retval.Add(sr);

                sr.Data = _itemGenService.Generate(gs, igd);
                sr.Quantity = rollData.QualityTypeId;
            }
            else
            {
                for (int i = 0; i < quantity; i++)
                {
                    SpawnResult sr = new SpawnResult();
                    sr.EntityId = spawnItem.EntityId;
                    sr.EntityTypeId = EntityTypes.Item;
                    sr.Quantity = 1;
                    sr.QualityTypeId = rollData.QualityTypeId;
                    sr.Level = rollData.Level;
                    retval.Add(sr);

                    sr.Data = _itemGenService.Generate(gs, igd);
                }
            }
            return retval;
        }
    }
}
