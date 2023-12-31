﻿using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Spawns;
using Genrpg.MapServer.Crafting;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Spells.Casting;

namespace Genrpg.MapServer.Items
{
    public interface IItemService : IService
    {
        UseItemResult UseItem(GameState gs, Character ch, Item item);

    }


    public class ItemService : IItemService
    {
        private ISpawnService _spawnService = null;
        private IServerCraftingService _craftingService = null;
        private IEntityService _entityService = null;
        private IMapObjectManager _objectManager = null;
        private IInventoryService _inventoryService = null;
        // This should call out to different functions in different parts of the code.
        // Eventually split these cases into separate functions.
        public UseItemResult UseItem(GameState gs, Character ch, Item item)
        {
            UseItemResult res = new UseItemResult() { ItemUsed = item, Success = false };
            if (item == null)
            {
                res.Message = "Missing item";
                return res;
            }

            if (item.UseEntityTypeId == EntityTypes.Recipe)
            {
                res = _craftingService.LearnRecipe(gs, ch, item);

            }
            else if (item.UseEntityTypeId == EntityTypes.Spawn)
            {
                RollData rollData = new RollData()
                {
                    Level = ch.Level,
                    QualityTypeId = item.QualityTypeId,
                    Times = 1
                };
                List<SpawnResult> newItems = _spawnService.Roll(gs, item.UseEntityId, rollData);
                if (newItems != null)
                {
                    _entityService.GiveRewards(gs, ch, newItems);
                }

                res.Success = true;
            }
            else if (item.UseEntityTypeId == EntityTypes.Spell)
            {
                if (_objectManager.GetUnit(ch.TargetId, out Unit targUnit))
                {
                    CastResult cr = new CastResult();
                    res.Success = true;
                }
            }

            if (res.Success)
            {
                _inventoryService.RemoveItemQuantity(gs, ch, item.Id, 1);
            }

            return res;
        }
    }
}
