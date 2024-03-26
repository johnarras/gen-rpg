
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.MapServer.Spawns;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Interactions.Messages;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.ServerShared.Achievements;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.GroundObjects.Settings;

namespace Genrpg.MapServer.InteractObject.MessageHandlers
{
    public class CompleteInteractHandler : BaseServerMapMessageHandler<CompleteInteract>
    {
        private ISpawnService _spawnService = null;

        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, CompleteInteract message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }

            _logService.Message("Finish Interact " + DateTime.UtcNow);
            string errorMessage = "";
            if (obj.ActionMessage == null)
            {
                errorMessage = "You aren't casting";
            }

            if (string.IsNullOrEmpty(errorMessage) && obj.ActionMessage != message)
            {
                errorMessage = "You aren't casting this";
            }

            MapObject target = null;
            if (string.IsNullOrEmpty(errorMessage) &&
                !_objectManager.GetObject(message.TargetId, out target))
            {
                errorMessage = "Target doesn't exist";
            }
            CompleteInteract targetAction = null;
            if (string.IsNullOrEmpty(errorMessage))
            {
                targetAction = target.OnActionMessage as CompleteInteract;
                if (targetAction == null || targetAction != message)
                {
                    errorMessage = "Target was busy";
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                pack.SendError(gs, obj, errorMessage);
                message.SetCancelled(true);
                if (obj.ActionMessage != null)
                {
                    obj.ActionMessage.SetCancelled(true);
                }
                if (targetAction != null)
                {
                    targetAction.SetCancelled(true);
                }
                return;
            }

            if (!message.IsSkillLoot)
            {
                GroundObjType gtype = gs.data.Get<GroundObjTypeSettings>(obj).Get(message.GroundObjTypeId);

                if (gtype != null && gtype.SpawnTableId > 0)
                {
                    List<SpawnItem> lootItems = new List<SpawnItem>();
                    lootItems.Add(new SpawnItem()
                    {
                        EntityTypeId = EntityTypes.Spawn,
                        EntityId = gtype.SpawnTableId,
                        MinQuantity = gtype.MinRolls,
                        MaxQuantity = gtype.MaxRolls,
                    });

                    RollData rollData = new RollData()
                    {
                        Level = message.Level,
                        QualityTypeId = QualityTypes.Common,
                        Times = 1,
                    };
                    List<SpawnResult> rewards = _spawnService.Roll(gs, lootItems, rollData);

                    if (rewards.Count > 0)
                    {
                        _entityService.GiveRewards(gs, ch, rewards);

                        SendRewards sendLoot = new SendRewards()
                        {
                            ShowPopup = true,
                            Rewards = rewards,
                        };
                        ch.AddMessage(sendLoot);
                    }
                }
            }
            else
            {
                if (_objectManager.GetUnit(message.TargetId, out Unit unit))
                {
                    if (unit.SkillLoot != null && unit.SkillLoot.Count > 0)
                    {
                        _entityService.GiveRewards(gs, ch, unit.SkillLoot);

                        SendRewards sendLoot = new SendRewards()
                        {
                            ShowPopup = true,
                            Rewards = unit.SkillLoot,
                        };
                        ch.AddMessage(sendLoot);
                        unit.SkillLoot = null;
                    }
                }
            }
            message.SetCancelled(true);
            obj.ActionMessage = null;
            target.OnActionMessage = null;
            _objectManager.RemoveObject(gs, target.Id, 0);
        }
    }
}
