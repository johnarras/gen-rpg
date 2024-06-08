
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interactions.Messages;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Crafting.Constants;
using Genrpg.Shared.Crafting.PlayerData.Crafting;
using Genrpg.Shared.Crafting.Settings.Crafters;
using Genrpg.Shared.GroundObjects.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.InteractObject.MessageHandlers
{
    public class InteractCommandHandler : BaseMapObjectServerMapMessageHandler<InteractCommand>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, InteractCommand message)
        {
            if (!_objectManager.GetObject(message.TargetId, out MapObject target))
            {
                pack.SendError(obj, "Object does not exist!");
                return;
            }

            if (obj.ActionMessage != null)
            {
                pack.SendError(obj, "You are already busy");
                return;
            }

            string actionName = "Gathering";
            string animName = "Gather";

            long crafterId = 0;

            float gatherSeconds = 0;

            long level = target.Level;
            int skillPoints = 0;
            long groundObjTypeId = 0;

            if (target.EntityTypeId == EntityTypes.GroundObject)
            {
                GroundObjType gtype = _gameData.Get<GroundObjTypeSettings>(obj).Get(target.EntityId);

                if (gtype == null)
                {
                    pack.SendError(obj, "Invalid object type");
                    return;
                }
                groundObjTypeId = gtype.IdKey;
                crafterId = gtype.CrafterTypeId;

                CrafterType ctype = _gameData.Get<CraftingSettings>(obj).Get(crafterId);
                if (ctype != null)
                {
                    actionName = ctype.GatherActionName;
                    animName = ctype.GatherAnimation;
                    if (obj is Character ch)
                    {
                        CraftingData cdata = ch.Get<CraftingData>();
                        skillPoints = cdata.Get(crafterId).GetSkillPoints(CraftingConstants.GatheringSkill);
                    }
                    gatherSeconds = ctype.GatherSeconds;
                }
                else
                {
                    crafterId = 0;
                }
            }

            OnStartCast startCast = obj.GetCachedMessage<OnStartCast>(true);
            startCast.CasterId = obj.Id;
            startCast.CastSeconds = gatherSeconds;
            startCast.CastingName = actionName;
            startCast.AnimName = animName;


            CompleteInteract completeInteract = new CompleteInteract();
            completeInteract.CasterId = obj.Id;
            completeInteract.TargetId = target.Id;
            completeInteract.CrafterTypeId = crafterId;
            completeInteract.Level = level;
            completeInteract.SkillPoints = skillPoints;
            completeInteract.GroundObjTypeId = groundObjTypeId;
            completeInteract.IsSkillLoot = message.IsSkillLoot;
            _logService.Message("Start Interact: " + DateTime.UtcNow + " " + gatherSeconds);
            lock (target.OnActionLock)
            {
                if (target.OnActionMessage != null && !target.OnActionMessage.IsCancelled())
                {
                    pack.SendError(obj, "Object is in use");
                    return;
                }
                else
                {
                    target.OnActionMessage = completeInteract;
                    obj.ActionMessage = completeInteract;
                }
            }


            _messageService.SendMessageNear(obj, startCast);

            _messageService.SendMessage(obj, completeInteract, gatherSeconds);
        }
    }
}
