﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Interactions.Messages;

namespace Genrpg.MapServer.Looting.MessageHandlers
{
    public class SkillLootCorpseHandler : BaseServerMapMessageHandler<SkillLootCorpse>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, SkillLootCorpse message)
        {

            if (!_objectManager.GetUnit(message.UnitId, out Unit unit))
            {
                pack.SendError(gs, obj, "Corpse does not exist!");
                return;
            }

            if (obj.ActionMessage != null)
            {
                pack.SendError(gs, obj, "You are already busy");
                return;
            }


            UnitType utype = gs.data.GetGameData<UnitSettings>().GetUnitType(unit.EntityId);
            if (utype == null)
            {
                pack.SendError(gs, obj, "Not a valid target");
                return;
            }

            TribeType tribeType = gs.data.GetGameData<UnitSettings>().GetTribeType(utype.TribeTypeId);

            if (tribeType == null)
            {
                pack.SendError(gs, obj, "Not a valid type");
                return;
            }
            CrafterType crafterType = gs.data.GetGameData<CraftingSettings>().GetCrafterType(tribeType.LootCrafterTypeId);

            if (crafterType == null)
            {
                pack.SendError(gs, obj, "This unit has no resources");
                return;
            }

            string actionName = crafterType.GatherActionName;
            string animName = crafterType.GatherAnimation;
            float gatherSeconds = crafterType.GatherSeconds;
            long level = unit.Level;
            int skillPoints = 0;

            if (obj is Character ch)
            {
                CrafterData cdata = ch.Get<CrafterData>();
                skillPoints = cdata.Get(crafterType.IdKey).GetSkillPoints(CraftingConstants.GatheringSkill);
            }

            if (unit.SkillLoot == null || unit.SkillLoot.Count < 1)
            {
                pack.SendError(gs, obj, "Target has no loot");
                return;
            }

            OnStartCast startCast = new OnStartCast()
            {
                CasterId = obj.Id,
                CastSeconds = gatherSeconds,
                CastingName = actionName,
                AnimName = animName,
            };

            CompleteInteract completeInteract = new CompleteInteract()
            {
                CasterId = obj.Id,
                TargetId = unit.Id,
                CrafterTypeId = crafterType.IdKey,
                Level = level,
                SkillPoints = skillPoints,
                GroundObjTypeId = 0,
                IsSkillLoot = true,
            };


            lock (unit.OnActionLock)
            {
                if (unit.OnActionMessage != null && !unit.OnActionMessage.IsCancelled())
                {
                    pack.SendError(gs, obj, "Object is in use");
                    return;
                }
                else
                {
                    unit.OnActionMessage = completeInteract;
                    obj.ActionMessage = completeInteract;
                }
            }

            _messageService.SendMessageNear(gs, obj, startCast);

            _messageService.SendMessage(gs, obj, completeInteract, gatherSeconds);

        }
    }
}