
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Levels.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using System.Threading;
using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Spawns;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Combat.Messages;

namespace Genrpg.MapServer.Units
{
    public class ServerUnitService : IServerUnitService
    {
        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private ISpawnService _spawnService = null;
        public async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public void CheckForDeath(GameState gs, SpellEffect eff, Unit targ)
        {
            if (targ.HasFlag(UnitFlags.IsDead))
            {
                return;
            }

            targ.AddFlag(UnitFlags.IsDead);

            UnitType utype = gs.data.GetGameData<UnitSettings>().GetUnitType(targ.EntityId);

            TribeType ttype = gs.data.GetGameData<UnitSettings>().GetTribeType(utype.TribeTypeId);

            Died died = new Died()
            {
                UnitId = targ.Id,
                FirstAttackerId = targ.GetFirstAttacker(),
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

            targ.Loot = _spawnService.Roll(gs, gs.data.GetGameData<SpawnSettings>().MonsterLootSpawnTableId, rollData);

            LevelData levelData = gs.data.GetGameData<LevelSettings>().GetLevel(targ.Level);

            if (levelData != null)
            {

                targ.Loot.Add(new SpawnResult()
                {
                    EntityTypeId = EntityType.Currency,
                    EntityId = CurrencyType.Money,
                    Quantity = MathUtils.LongRange(1, levelData.KillMoney * 2, gs.rand),
                });
            }

            if (utype.LootItems != null)
            {
                targ.Loot.AddRange(_spawnService.Roll(gs, utype.LootItems, rollData));
            }

            // Quest loot? need list of quests from caster?

            if (utype.InteractLootItems != null)
            {
                targ.SkillLoot = _spawnService.Roll(gs, utype.InteractLootItems, rollData);
            }

            if (ttype != null)
            {
                targ.Loot.AddRange(_spawnService.Roll(gs, ttype.LootItems, rollData));
                targ.SkillLoot.AddRange(_spawnService.Roll(gs, ttype.InteractLootItems, rollData));
            }

            died.Loot = targ.Loot;
            died.SkillLoot = targ.SkillLoot;

            _messageService.SendMessageNear(gs, targ, died, MessageConstants.DefaultGridDistance * 2);

            Killed killed = new Killed()
            {
                UnitTypeId = targ.EntityTypeId,
                FactionTypeId = targ.FactionTypeId,
                Level = targ.Level,
                NPCTypeId = targ.NPCTypeId,
                ZoneId = targ.ZoneId,
                UnitId = targ.Id,
            };

            if (_objectManager.GetUnit(eff.CasterId, out Unit killerUnit))
            {
                _messageService.SendMessage(gs, killerUnit, killed);
            }

            _objectManager.RemoveObject(targ.Id, UnitConstants.CorpseDespawnSeconds);

        }
    }
}
