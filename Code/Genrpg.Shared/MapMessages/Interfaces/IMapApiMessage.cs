using Genrpg.Shared.Achievements.Messages;
using Genrpg.Shared.Chat.Messages;
using Genrpg.Shared.Combat.Messages;
using Genrpg.Shared.Currencies.Messages;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Interactions.Messages;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Levels.Messages;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Messages;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Networking.Messages;
using Genrpg.Shared.NPCs.Messages;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.Quests.Messages;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.UserCoins.Messages;
using Genrpg.Shared.WhoList.Messages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace Genrpg.Shared.MapMessages.Interfaces
{
    [Union(0,typeof(OnUpdatePos))]
    [Union(1,typeof(FX))]
    [Union(2,typeof(StatUpd))]
    [Union(3,typeof(OnUpdateAchievement))]
    [Union(4,typeof(OnChatMessage))]
    [Union(5,typeof(SendChatMessage))]
    [Union(6,typeof(Died))]
    [Union(7,typeof(InterruptCast))]
    [Union(8,typeof(OnAddCurrency))]
    [Union(9,typeof(ErrorMessage))]
    [Union(10,typeof(CompleteInteract))]
    [Union(11,typeof(InteractCommand))]
    [Union(12,typeof(BuyItem))]
    [Union(13,typeof(EquipItem))]
    [Union(14,typeof(OnAddItem))]
    [Union(15,typeof(OnEquipItem))]
    [Union(16,typeof(OnRemoveItem))]
    [Union(17,typeof(OnUnequipItem))]
    [Union(18,typeof(OnUpdateItem))]
    [Union(19,typeof(SellItem))]
    [Union(20,typeof(UnequipItem))]
    [Union(21,typeof(NewLevel))]
    [Union(22,typeof(ClearLoot))]
    [Union(23,typeof(LootCorpse))]
    [Union(24,typeof(SendRewards))]
    [Union(25,typeof(SkillLootCorpse))]
    [Union(26,typeof(DespawnObject))]
    [Union(27,typeof(GetSpawnedObject))]
    [Union(28,typeof(OnSpawn))]
    [Union(29,typeof(SendSpawn))]
    [Union(30,typeof(MapObjectCounts))]
    [Union(31,typeof(ServerMessageCounts))]
    [Union(32,typeof(UpdatePos))]
    [Union(33,typeof(ConnMessageCounts))]
    [Union(34,typeof(GetNPCStatus))]
    [Union(35,typeof(OnGetNPCStatus))]
    [Union(36,typeof(Ping))]
    [Union(37,typeof(AddPlayer))]
    [Union(38,typeof(OnFinishLoadPlayer))]
    [Union(39,typeof(SaveDirty))]
    [Union(40,typeof(GetNPCQuests))]
    [Union(41,typeof(OnGetNPCQuests))]
    [Union(42,typeof(CraftSpell))]
    [Union(43,typeof(DeleteSpell))]
    [Union(44,typeof(OnCraftSpell))]
    [Union(45,typeof(OnDeleteSpell))]
    [Union(46,typeof(OnRemoveActionBarItem))]
    [Union(47,typeof(OnSetActionBarItem))]
    [Union(48,typeof(RemoveActionBarItem))]
    [Union(49,typeof(SetActionBarItem))]
    [Union(50,typeof(ActiveSpellEffect))]
    [Union(51,typeof(CastingSpell))]
    [Union(52,typeof(CastSpell))]
    [Union(53,typeof(CombatText))]
    [Union(54,typeof(OnAddEffect))]
    [Union(55,typeof(OnRemoveEffect))]
    [Union(56,typeof(OnStartCast))]
    [Union(57,typeof(OnStopCast))]
    [Union(58,typeof(OnUpdateEffect))]
    [Union(59,typeof(ResendSpell))]
    [Union(60,typeof(Regen))]
    [Union(61,typeof(OnSetTarget))]
    [Union(62,typeof(OnTargetIsDead))]
    [Union(63,typeof(SetTarget))]
    [Union(64,typeof(OnAddUserCoin))]
    [Union(65,typeof(GetWhoList))]
    [Union(66,typeof(OnGetWhoList))]
    public interface IMapApiMessage : IMapMessage
    {
    }
}
