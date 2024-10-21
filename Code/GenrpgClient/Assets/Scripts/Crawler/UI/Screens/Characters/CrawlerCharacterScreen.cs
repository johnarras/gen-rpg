
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Exploring;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.Screens.Characters
{
    public class CrawlerCharacterScreen : CharacterScreen
    {
        ICrawlerService _crawlerService;
        ICrawlerStatService _crawlerStatService;

        ECrawlerStates _prevState = ECrawlerStates.GuildMain;

        protected override bool CalcStatsOnEquipUnequip() { return false; }
        protected override string GetStatSubdirectory() { return "CrawlerUnits"; }
        protected override bool ShowZeroStats() { return false; }

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            if (data is CrawlerCharacterScreenData csd)
            {
                _unit = csd.Unit;
                _prevState = csd.PrevState;
            }
            await base.OnStartOpen(data, token);
        }

        protected override void OnStartClose()
        {
            _crawlerService.ChangeState(_prevState, _token);
            base.OnStartClose();
        }

        protected override void TryEquip(Item origItem, long equipSlotId)
        {
            InventoryData inventoryData = _unit.Get<InventoryData>();

            List<Item> equipment = inventoryData.GetAllEquipment();
            if (_inventoryService.EquipItem(_unit, origItem.Id, equipSlotId, false))
            {
                _inventoryService.UnequipItem(_unit, origItem.Id, false);

                Item newItem = SerializationUtils.SafeMakeCopy(origItem);
                newItem.EquipSlotId = equipSlotId;
                OnEquip(new OnEquipItem() { Item = newItem, UnitId = _unit.Id });

                List<Item> removedItems = equipment.Except(inventoryData.GetAllEquipment()).ToList();

                foreach (Item item in removedItems)
                {
                    Items.InitIcon(item, _token);
                }

                CopyDataBack();
            }
        }

        protected override void ShowStats()
        {
            _crawlerStatService.CalcUnitStats(_crawlerService.GetParty(), _unit as CrawlerUnit, false);
            base.ShowStats();
        }

        protected override void TryUnequip(Item item)
        {
            OnUnequip(new OnUnequipItem() { UnitId = _unit.Id, ItemId = item.Id });
            CopyDataBack();
        }

        private void CopyDataBack()
        {
            PartyData partyData = _crawlerService.GetParty();
            PartyMember member = _unit as PartyMember;

            InventoryData invenData = member.Get<InventoryData>();

            partyData.Inventory = invenData.GetAllInventory();
            member.Equipment = invenData.GetAllEquipment();

            ShowStats();
        }
    }
}
