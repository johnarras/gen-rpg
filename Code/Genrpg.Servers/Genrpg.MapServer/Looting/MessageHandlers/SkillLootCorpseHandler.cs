using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Interactions.Messages;
using Genrpg.Shared.Crafting.Constants;
using Genrpg.Shared.Crafting.PlayerData.Crafting;
using Genrpg.Shared.Crafting.Settings.Crafters;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.MapMessaging.MessageHandlers;

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


            UnitType utype = _gameData.Get<UnitSettings>(obj).Get(unit.EntityId);
            if (utype == null)
            {
                pack.SendError(gs, obj, "Not a valid target");
                return;
            }

            TribeType tribeType = _gameData.Get<TribeSettings>(obj).Get(utype.TribeTypeId);

            if (tribeType == null)
            {
                pack.SendError(gs, obj, "Not a valid type");
                return;
            }
            CrafterType crafterType = _gameData.Get<CraftingSettings>(obj).Get(tribeType.LootCrafterTypeId);

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
                CraftingData cdata = ch.Get<CraftingData>();
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

            _messageService.SendMessageNear(obj, startCast);

            _messageService.SendMessage(obj, completeInteract, gatherSeconds);

        }
    }
}
