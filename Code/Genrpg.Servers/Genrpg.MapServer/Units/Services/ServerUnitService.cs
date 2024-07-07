
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using System.Threading;
using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Combat.Messages;
using Genrpg.Shared.Characters.PlayerData;
using System.Linq;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.ServerShared.Achievements;
using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.MapServer.Spawns.Services;
using Genrpg.Shared.Units.Settings;

namespace Genrpg.MapServer.Units.Services
{
    public class ServerUnitService : IServerUnitService
    {
        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private ISpawnService _spawnService = null;
        private IGameData _gameData = null;
        private IAchievementService _achievementService = null;

        public void CheckForDeath(IRandom rand, ActiveSpellEffect eff, Unit targ)
        {
            if (targ.HasFlag(UnitFlags.IsDead))
            {
                return;
            }

            targ.AddFlag(UnitFlags.IsDead);

            UnitType utype = _gameData.Get<UnitSettings>(targ).Get(targ.EntityId);

            TribeType ttype = _gameData.Get<TribeSettings>(targ).Get(utype.TribeTypeId);

            AttackerInfo firstAttacker = targ.GetFirstAttacker();

            if (firstAttacker == null)
            {
                targ.AddAttacker(eff.CasterId, eff.CasterGroupId);
                firstAttacker = targ.GetFirstAttacker();
            }

            Died died = new Died()
            {
                UnitId = targ.Id,
                FirstAttacker = firstAttacker,
            };

            targ.Loot = new List<SpawnResult>();
            targ.SkillLoot = new List<SpawnResult>();

            RollData rollData = new RollData()
            {
                Level = targ.Level,
                Depth = 0,
                QualityTypeId = targ.QualityTypeId,
                Times = 1,
            };

            if (firstAttacker != null)
            {
                targ.SkillLoot = new List<SpawnResult>();

                targ.Loot = _spawnService.Roll(rand, _gameData.Get<SpawnSettings>(targ).MonsterLootSpawnTableId, rollData);
                LevelInfo levelData = _gameData.Get<LevelSettings>(targ).Get(targ.Level);

                if (levelData != null)
                {
                    targ.Loot.Add(new SpawnResult()
                    {
                        EntityTypeId = EntityTypes.Currency,
                        EntityId = CurrencyTypes.Money,
                        Quantity = MathUtils.LongRange(levelData.KillMoney / 2, levelData.KillMoney * 3 / 2, rand),
                    });
                }

                if (utype.LootItems != null)
                {
                    targ.Loot.AddRange(_spawnService.Roll(rand, utype.LootItems, rollData));
                }
                // Quest loot? need list of quests from caster?

                if (utype.InteractLootItems != null)
                {
                    targ.SkillLoot = _spawnService.Roll(rand, utype.InteractLootItems, rollData);
                }

                if (ttype != null)
                {
                    targ.Loot.AddRange(_spawnService.Roll(rand, ttype.LootItems, rollData));
                    targ.SkillLoot.AddRange(_spawnService.Roll(rand, ttype.InteractLootItems, rollData));
                }

                foreach (AttackerInfo info in targ.GetAttackers())
                {

                    if (_objectManager.GetChar(info.AttackerId, out Character ch))
                    {
                        _achievementService.UpdateAchievement(ch, AchievementConstants.KillMonsterStartId + utype.IdKey, 1);
                    }
                }

            }

            died.Loot = targ.Loot;
            died.SkillLoot = targ.SkillLoot;

            _messageService.SendMessageNear(targ, died, MessageConstants.DefaultGridDistance * 2);

            Killed killed = new Killed()
            {
                UnitTypeId = targ.EntityTypeId,
                FactionTypeId = targ.FactionTypeId,
                Level = targ.Level,
                ObjId = targ.Id,
                ZoneId = targ.ZoneId,
                UnitId = targ.Id,

            };

            if (_objectManager.GetUnit(eff.CasterId, out Unit killerUnit))
            {
                _messageService.SendMessage(killerUnit, killed);
            }

            _objectManager.RemoveObject(rand, targ.Id, UnitConstants.CorpseDespawnSeconds);

        }
    }
}
