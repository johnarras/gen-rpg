using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guild.CreateMember
{
    public class ChooseNameHelper : BaseStateHelper
    {
        private ILootGenService _lootGenService;
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseName; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            stateData.WorldSpriteName = member.PortraitName;

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.ChoosePortrait,
                extraData: member));

            stateData.AddInputField("Name: ", delegate (string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    member.Name = text;
                    _crawlerService.GetParty().Members.Add(member);

                    IReadOnlyList<ItemType> itemTypes = _gameData.Get<ItemTypeSettings>(_gs.ch).GetData();

                    List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);

                    List<RoleBonus> weaponBonuses = new List<RoleBonus>();

                    foreach (Role role in roles)
                    {
                        weaponBonuses.AddRange(role.Bonuses.Where(x => x.EntityTypeId == EntityTypes.Item));
                    }

                    List<long> okWeaponTypes = weaponBonuses.Where(x => x.EntityTypeId == EntityTypes.Item).Select(x => x.EntityId).ToList();

                    List<ItemType> okMelee = new List<ItemType>();
                    List<ItemType> okRanged = itemTypes.Where(x => x.EquipSlotId == EquipSlots.Ranged).ToList();

                    foreach (long wt in okWeaponTypes)
                    {
                        ItemType itype = itemTypes.FirstOrDefault(x => x.IdKey == wt);
                        if (itype != null)
                        {
                            if (itype.EquipSlotId == EquipSlots.MainHand)
                            {
                                okMelee.Add(itype);
                            }
                            else if (itype.EquipSlotId == EquipSlots.Ranged)
                            {
                                okRanged.Add(itype);
                            }
                        }
                    }

                    if (okMelee.Count > 0)
                    {
                        ItemGenData igd = new ItemGenData()
                        {
                            Level = 1,
                            ItemTypeId = okMelee[_rand.Next() % okMelee.Count].IdKey,
                        };
                        Item newItem = _lootGenService.GenerateItem(igd);
                        if (newItem != null)
                        {
                            member.Equipment.Add(newItem);
                            newItem.EquipSlotId = EquipSlots.MainHand;
                        }
                    }
                    if (okRanged.Count > 0)
                    {
                        ItemGenData igd = new ItemGenData()
                        {
                            Level = 1,
                            ItemTypeId = okRanged[_rand.Next() % okRanged.Count].IdKey,
                        };
                        Item newItem = _lootGenService.GenerateItem(igd);
                        if (newItem != null)
                        {
                            member.Equipment.Add(newItem);
                            newItem.EquipSlotId = EquipSlots.Ranged;
                        }
                    }

                    _spellService.SetupCombatData(_crawlerService.GetParty(), member);
                    _statService.CalcUnitStats(_crawlerService.GetParty(), member, true);

                    _crawlerService.SaveGame();
                    _crawlerService.ChangeState(ECrawlerStates.GuildMain, token);
                }
            });


            await Task.CompletedTask;
            return stateData;
        }
    }
}
