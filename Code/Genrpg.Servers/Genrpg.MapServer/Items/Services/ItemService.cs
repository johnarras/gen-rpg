using Genrpg.MapServer.Maps;
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
using System.Linq;
using System;
using System.Threading;
using Genrpg.MapServer.Crafting.Services;
using Genrpg.MapServer.Spawns.Services;
using Genrpg.MapServer.Trades.Services;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Items.Services
{
    public interface IItemService : IInjectable
    {
        UseItemResult UseItem(IRandom rand, Character ch, Item item);

    }


    public class ItemService : IItemService
    {
        private ISpawnService _spawnService = null;
        private IServerCraftingService _craftingService = null;
        private IEntityService _entityService = null;
        private IMapObjectManager _objectManager = null;
        private IInventoryService _inventoryService = null;
        private ITradeService _tradeService = null;

        // This should call out to different functions in different parts of the code.
        // Eventually split these cases into separate functions.
        public UseItemResult UseItem(IRandom rand, Character ch, Item item)
        {
            return _tradeService.SafeModifyObject(ch, delegate
            {
                return UseItemInternal(rand, ch, item);
            },
            new UseItemResult() { ItemUsed = item, Success = false });
        }

        private UseItemResult UseItemInternal (IRandom rand, Character ch, Item item)
        { 
            UseItemResult res = new UseItemResult() { ItemUsed = item, Success = false };
            if (item == null)
            {
                res.Message = "Missing item";
                return res;
            }
            bool shouldRemoveItem = false;

            ItemProc theProc = null;

            ItemProc recipeProc = item.Procs.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Recipe);

            if (recipeProc != null)
            {
                theProc = recipeProc;
                shouldRemoveItem = true;
                res = _craftingService.LearnRecipe(rand, ch, item);
            }

            if (theProc == null)
            {
                ItemProc spawnProc = item.Procs.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Spawn);

                if (spawnProc != null)
                {
                    shouldRemoveItem = true;
                    theProc = spawnProc;
                    RollData rollData = new RollData()
                    {
                        Level = ch.Level,
                        QualityTypeId = item.QualityTypeId,
                        Times = 1
                    };
                    List<SpawnResult> newItems = _spawnService.Roll(rand, spawnProc.EntityId, rollData);
                    if (newItems != null)
                    {
                        _entityService.GiveRewards(rand, ch, newItems);
                    }

                    res.Success = true;
                }
            }

            if (theProc == null)
            {
                ItemProc spellProc = item.Procs.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Spell);
                theProc = spellProc;
                if (spellProc != null)
                {
                    if (spellProc.MaxCharges > 0 && spellProc.CurrCharges < 1)
                    {
                        res.Message = "Out of Charges";
                        return res;
                    }

                    if (spellProc.CooldownSeconds > 0 && (DateTime.UtcNow - spellProc.LastUsedTime).TotalSeconds < spellProc.CooldownSeconds)
                    {
                        res.Message = "Item is on cooldown";
                        return res;
                    }

                    if (_objectManager.GetUnit(ch.TargetId, out Unit targUnit))
                    {
                        CastResult cr = new CastResult();
                        res.Success = true;
                    }
                    spellProc.CurrCharges--;
                    spellProc.LastUsedTime = DateTime.UtcNow;
                }
            }

            if (theProc != null && res.Success)
            {
                if (shouldRemoveItem)
                {
                    _inventoryService.RemoveItemQuantity(ch, item.Id, 1);
                }
            }

            return res;
        }
    }
}
