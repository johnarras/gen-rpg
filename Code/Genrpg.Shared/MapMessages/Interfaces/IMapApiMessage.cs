using Genrpg.Shared.Achievements.Messages;
using Genrpg.Shared.Chat.Messages;
using Genrpg.Shared.Combat.Messages;
using Genrpg.Shared.Currencies.Messages;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Ftue.Messages;
using Genrpg.Shared.GameSettings.Messages;
using Genrpg.Shared.Interactions.Messages;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Levels.Messages;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Messages;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Networking.Messages;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.Quests.Messages;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.Settings.Effects;
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
    [Union(10,typeof(CompleteFtueStepMessage))]
    [Union(11,typeof(OnRefreshGameSettings))]
    [Union(12,typeof(CompleteInteract))]
    [Union(13,typeof(InteractCommand))]
    [Union(14,typeof(BuyItem))]
    [Union(15,typeof(EquipItem))]
    [Union(16,typeof(OnAddItem))]
    [Union(17,typeof(OnEquipItem))]
    [Union(18,typeof(OnRemoveItem))]
    [Union(19,typeof(OnUnequipItem))]
    [Union(20,typeof(OnUpdateItem))]
    [Union(21,typeof(SellItem))]
    [Union(22,typeof(UnequipItem))]
    [Union(23,typeof(NewLevel))]
    [Union(24,typeof(ClearLoot))]
    [Union(25,typeof(LootCorpse))]
    [Union(26,typeof(SendRewards))]
    [Union(27,typeof(SkillLootCorpse))]
    [Union(28,typeof(DespawnObject))]
    [Union(29,typeof(GetMapObjectStatus))]
    [Union(30,typeof(GetSpawnedObject))]
    [Union(31,typeof(OnGetMapObjectStatus))]
    [Union(32,typeof(OnSpawn))]
    [Union(33,typeof(SendSpawn))]
    [Union(34,typeof(MapObjectCounts))]
    [Union(35,typeof(ServerMessageCounts))]
    [Union(36,typeof(UpdatePos))]
    [Union(37,typeof(ConnMessageCounts))]
    [Union(38,typeof(Ping))]
    [Union(39,typeof(AddPlayer))]
    [Union(40,typeof(OnFinishLoadPlayer))]
    [Union(41,typeof(SaveDirty))]
    [Union(42,typeof(GetQuests))]
    [Union(43,typeof(OnGetQuests))]
    [Union(44,typeof(CraftSpell))]
    [Union(45,typeof(DeleteSpell))]
    [Union(46,typeof(OnCraftSpell))]
    [Union(47,typeof(OnDeleteSpell))]
    [Union(48,typeof(OnRemoveActionBarItem))]
    [Union(49,typeof(OnSetActionBarItem))]
    [Union(50,typeof(RemoveActionBarItem))]
    [Union(51,typeof(SetActionBarItem))]
    [Union(52,typeof(CastingSpell))]
    [Union(53,typeof(CastSpell))]
    [Union(54,typeof(CombatText))]
    [Union(55,typeof(OnAddEffect))]
    [Union(56,typeof(OnRemoveEffect))]
    [Union(57,typeof(OnStartCast))]
    [Union(58,typeof(OnStopCast))]
    [Union(59,typeof(OnUpdateEffect))]
    [Union(60,typeof(ResendSpell))]
    [Union(61,typeof(ActiveSpellEffect))]
    [Union(62,typeof(Regen))]
    [Union(63,typeof(OnSetTarget))]
    [Union(64,typeof(OnTargetIsDead))]
    [Union(65,typeof(SetTarget))]
    [Union(66,typeof(OnAddUserCoin))]
    [Union(67,typeof(GetWhoList))]
    [Union(68,typeof(OnGetWhoList))]
    public interface IMapApiMessage : IMapMessage
    {
    }
}
