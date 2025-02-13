using Genrpg.Shared.Achievements.Messages;
using Genrpg.Shared.Chat.Messages;
using Genrpg.Shared.Combat.Messages;
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
using Genrpg.Shared.Rewards.Messages;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Trades.Messages;
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
    [Union(8,typeof(ErrorMessage))]
    [Union(9,typeof(CompleteFtueStepMessage))]
    [Union(10,typeof(OnRefreshGameSettings))]
    [Union(11,typeof(CompleteInteract))]
    [Union(12,typeof(InteractCommand))]
    [Union(13,typeof(BuyItem))]
    [Union(14,typeof(EquipItem))]
    [Union(15,typeof(OnAddItem))]
    [Union(16,typeof(OnEquipItem))]
    [Union(17,typeof(OnRemoveItem))]
    [Union(18,typeof(OnUnequipItem))]
    [Union(19,typeof(OnUpdateItem))]
    [Union(20,typeof(SellItem))]
    [Union(21,typeof(UnequipItem))]
    [Union(22,typeof(NewLevel))]
    [Union(23,typeof(ClearLoot))]
    [Union(24,typeof(LootCorpse))]
    [Union(25,typeof(SendRewards))]
    [Union(26,typeof(SkillLootCorpse))]
    [Union(27,typeof(DespawnObject))]
    [Union(28,typeof(GetMapObjectStatus))]
    [Union(29,typeof(GetSpawnedObject))]
    [Union(30,typeof(OnGetMapObjectStatus))]
    [Union(31,typeof(OnSpawn))]
    [Union(32,typeof(SendSpawn))]
    [Union(33,typeof(MapObjectCounts))]
    [Union(34,typeof(ServerMessageCounts))]
    [Union(35,typeof(OnAddToGrid))]
    [Union(36,typeof(UpdatePos))]
    [Union(37,typeof(ConnMessageCounts))]
    [Union(38,typeof(Ping))]
    [Union(39,typeof(AddPlayer))]
    [Union(40,typeof(OnFinishLoadPlayer))]
    [Union(41,typeof(SaveDirty))]
    [Union(42,typeof(GetQuests))]
    [Union(43,typeof(OnGetQuests))]
    [Union(44,typeof(OnAddQuantityReward))]
    [Union(45,typeof(CraftSpell))]
    [Union(46,typeof(DeleteSpell))]
    [Union(47,typeof(OnCraftSpell))]
    [Union(48,typeof(OnDeleteSpell))]
    [Union(49,typeof(OnRemoveActionBarItem))]
    [Union(50,typeof(OnSetActionBarItem))]
    [Union(51,typeof(RemoveActionBarItem))]
    [Union(52,typeof(SetActionBarItem))]
    [Union(53,typeof(CastingSpell))]
    [Union(54,typeof(CastSpell))]
    [Union(55,typeof(CombatText))]
    [Union(56,typeof(OnAddEffect))]
    [Union(57,typeof(OnRemoveEffect))]
    [Union(58,typeof(OnStartCast))]
    [Union(59,typeof(OnStopCast))]
    [Union(60,typeof(OnUpdateEffect))]
    [Union(61,typeof(ResendSpell))]
    [Union(62,typeof(ActiveSpellEffect))]
    [Union(63,typeof(Regen))]
    [Union(64,typeof(OnSetTarget))]
    [Union(65,typeof(OnTargetIsDead))]
    [Union(66,typeof(SetTarget))]
    [Union(67,typeof(AcceptTrade))]
    [Union(68,typeof(CancelTrade))]
    [Union(69,typeof(OnAcceptTrade))]
    [Union(70,typeof(OnCancelTrade))]
    [Union(71,typeof(OnCompleteTrade))]
    [Union(72,typeof(OnStartTrade))]
    [Union(73,typeof(OnUpdateTrade))]
    [Union(74,typeof(StartTrade))]
    [Union(75,typeof(UpdateTrade))]
    [Union(76,typeof(OnAddUserCoin))]
    [Union(77,typeof(GetWhoList))]
    [Union(78,typeof(OnGetWhoList))]
    public interface IMapApiMessage : IMapMessage
    {
    }
}
