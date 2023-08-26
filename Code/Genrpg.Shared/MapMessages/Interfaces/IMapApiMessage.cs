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
    [Union(3,typeof(OnChatMessage))]
    [Union(4,typeof(SendChatMessage))]
    [Union(5,typeof(Died))]
    [Union(6,typeof(InterruptCast))]
    [Union(7,typeof(OnAddCurrency))]
    [Union(8,typeof(ErrorMessage))]
    [Union(9,typeof(CompleteInteract))]
    [Union(10,typeof(InteractCommand))]
    [Union(11,typeof(BuyItem))]
    [Union(12,typeof(EquipItem))]
    [Union(13,typeof(OnAddItem))]
    [Union(14,typeof(OnEquipItem))]
    [Union(15,typeof(OnRemoveItem))]
    [Union(16,typeof(OnUnequipItem))]
    [Union(17,typeof(OnUpdateItem))]
    [Union(18,typeof(SellItem))]
    [Union(19,typeof(UnequipItem))]
    [Union(20,typeof(NewLevel))]
    [Union(21,typeof(ClearLoot))]
    [Union(22,typeof(LootCorpse))]
    [Union(23,typeof(SendRewards))]
    [Union(24,typeof(SkillLootCorpse))]
    [Union(25,typeof(DespawnObject))]
    [Union(26,typeof(GetSpawnedObject))]
    [Union(27,typeof(OnSpawn))]
    [Union(28,typeof(SendSpawn))]
    [Union(29,typeof(MapObjectCounts))]
    [Union(30,typeof(ServerMessageCounts))]
    [Union(31,typeof(UpdatePos))]
    [Union(32,typeof(ConnMessageCounts))]
    [Union(33,typeof(GetNPCStatus))]
    [Union(34,typeof(OnGetNPCStatus))]
    [Union(35,typeof(Ping))]
    [Union(36,typeof(AddPlayer))]
    [Union(37,typeof(OnFinishLoadPlayer))]
    [Union(38,typeof(SaveDirty))]
    [Union(39,typeof(GetNPCQuests))]
    [Union(40,typeof(OnGetNPCQuests))]
    [Union(41,typeof(CraftSpell))]
    [Union(42,typeof(DeleteSpell))]
    [Union(43,typeof(OnCraftSpell))]
    [Union(44,typeof(OnDeleteSpell))]
    [Union(45,typeof(OnRemoveActionBarItem))]
    [Union(46,typeof(OnSetActionBarItem))]
    [Union(47,typeof(RemoveActionBarItem))]
    [Union(48,typeof(SetActionBarItem))]
    [Union(49,typeof(SpellEffect))]
    [Union(50,typeof(CastingSpell))]
    [Union(51,typeof(CastSpell))]
    [Union(52,typeof(CombatText))]
    [Union(53,typeof(OnAddEffect))]
    [Union(54,typeof(OnRemoveEffect))]
    [Union(55,typeof(OnStartCast))]
    [Union(56,typeof(OnStopCast))]
    [Union(57,typeof(OnUpdateEffect))]
    [Union(58,typeof(ResendSpell))]
    [Union(59,typeof(SendSpell))]
    [Union(60,typeof(SpellHit))]
    [Union(61,typeof(Regen))]
    [Union(62,typeof(OnSetTarget))]
    [Union(63,typeof(OnTargetIsDead))]
    [Union(64,typeof(SetTarget))]
    [Union(65,typeof(OnAddUserCoin))]
    public interface IMapApiMessage : IMapMessage
    {
    }
}
