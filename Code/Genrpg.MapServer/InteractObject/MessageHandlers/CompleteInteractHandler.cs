
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.MapServer.Spawns;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Interactions.Messages;
using Genrpg.Shared.Loot.Messages;

namespace Genrpg.MapServer.InteractObject.MessageHandlers
{
    public class CompleteInteractHandler : BaseServerMapMessageHandler<CompleteInteract>
    {
        private ISpawnService _spawnService;

        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, CompleteInteract message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }


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
                GroundObjType gtype = gs.data.GetGameData<ProcGenSettings>().GetGroundObjType(message.GroundObjTypeId);

                if (gtype != null && gtype.SpawnTableId > 0)
                {
                    List<SpawnItem> lootItems = new List<SpawnItem>();
                    lootItems.Add(new SpawnItem()
                    {
                        EntityTypeId = EntityType.Spawn,
                        EntityId = gtype.SpawnTableId,
                        MinQuantity = gtype.MinRolls,
                        MaxQuantity = gtype.MaxRolls,
                    });

                    RollData rollData = new RollData()
                    {
                        Level = message.Level,
                        QualityTypeId = QualityType.Common,
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
            pack.SendError(gs, obj, errorMessage);
            message.SetCancelled(true);
            obj.ActionMessage = null;
            target.OnActionMessage = null;
            _objectManager.RemoveObject(target.Id, 0);
        }
    }
}
