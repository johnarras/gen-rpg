
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
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.Slots;
using Assets.Scripts.UI.Crawler.StatusUI;
using UnityEngine;
using Assets.Scripts.Assets.Textures;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Units.Settings;
using Assets.Scripts.ClientEvents;

namespace Assets.Scripts.Crawler.UI.Screens.Characters
{
    public class CrawlerCharacterScreen : CharacterScreen
    {
        ICrawlerService _crawlerService;
        ICrawlerStatService _crawlerStatService;
        IInfoService _infoService;
        IRoleService _roleService;

        public AnimatedSprite Image;
        public GText NameText;
        public GText LevelText;
        public GText RaceText;
        public GText ClassText;
        public GText SummonText;
        public GText TiersText;

        protected override bool CalcStatsOnEquipUnequip() { return false; }
        protected override string GetStatSubdirectory() { return "CrawlerUnits"; }
        protected override bool ShowZeroStats() { return false; }


        protected PartyMember _partyMember;



        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            _dispatcher.AddListener<CrawlerCharacterScreenData>(OnScreenData, GetToken());

            IReadOnlyList<EquipSlot> equipSlots = _gameData.Get<EquipSlotSettings>(_gs.ch).GetData();

            foreach (EquipSlot equipSlot in equipSlots)
            {
                if (!equipSlot.IsCrawlerSlot)
                {
                    EquipSlotIcon icon = EquipmentIcons.FirstOrDefault(x => x.EquipSlotId == equipSlot.IdKey);
                    if (icon != null)
                    {
                        _clientEntityService.SetActive(icon, false);
                    }
                }
            }
            if (data is CrawlerCharacterScreenData csd)
            {
                OnScreenData(csd);
            }

            await base.OnStartOpen(data, token);
        }

        private void OnScreenData(CrawlerCharacterScreenData csd)
        {
            _unit = csd.Unit;
            _partyMember = csd.Unit;

            PartyData partyData = _crawlerService.GetParty();

            InventoryData idata = _partyMember.Get<InventoryData>();

            idata.SetInvenEquip(partyData.Inventory, _partyMember.Equipment);

            Image.SetImage(_partyMember.PortraitName);
            _uiService.SetText(NameText, _unit.Name);

            List<Role> allRoles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(_unit.Roles);

            _uiService.SetText(LevelText, "Level " + _unit.Level);


            Role raceRole = allRoles.FirstOrDefault(x => x.RoleCategoryId == RoleCategories.Origin);


            if (raceRole != null)
            {
                _uiService.SetText(RaceText, _infoService.CreateInfoLink(raceRole));
            }


            List<Role> classRoles = allRoles.Where(x => x.RoleCategoryId == RoleCategories.Class).ToList();


            StringBuilder sb = new StringBuilder();
            foreach (Role classRole in classRoles)
            {
                sb.Append(_infoService.CreateInfoLink(classRole) + " ");
            }

            _uiService.SetText(ClassText, sb.ToString());


            sb.Clear();
            if (_partyMember.Summons.Count > 0)
            {
                foreach (PartySummon summon in _partyMember.Summons)
                {
                    sb.Append(_infoService.CreateInfoLink(_gameData.Get<UnitSettings>(_gs.ch).Get(summon.UnitTypeId)) + " ");
                }
            }
            _uiService.SetText(SummonText, sb.ToString());

        }

        protected override void OnStartClose()
        {
            _dispatcher.Dispatch(new HideInfoPanelEvent());
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
