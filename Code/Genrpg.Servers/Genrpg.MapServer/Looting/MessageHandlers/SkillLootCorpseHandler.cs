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
using Genrpg.Shared.Utils;
using Genrpg.Shared.Units.Settings;

namespace Genrpg.MapServer.Looting.MessageHandlers
{
    public class SkillLootCorpseHandler : BaseUnitServerMapMessageHandler<SkillLootCorpse>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Unit looter, SkillLootCorpse message)
        {

            if (looter.ActionMessage != null)
            {
                pack.SendError(looter, "You are already busy");
                return;
            }

            if (!_objectManager.GetUnit(message.UnitId, out Unit target))
            {
                pack.SendError(looter, "Target does not exist");
                return;
            }

            if (target.SkillLoot == null || target.SkillLoot.Count < 1)
            {
                pack.SendError(looter, "Target has no loot");
                return;
            }

            UnitType utype = _gameData.Get<UnitSettings>(target).Get(target.EntityId);
            if (utype == null)
            {
                pack.SendError(looter, "Not a valid target");
                return;
            }

            TribeType tribeType = _gameData.Get<TribeSettings>(target).Get(utype.TribeTypeId);

            if (tribeType == null)
            {
                pack.SendError(looter, "Not a valid type");
                return;
            }
            CrafterType crafterType = _gameData.Get<CraftingSettings>(looter).Get(tribeType.LootCrafterTypeId);

            if (crafterType == null)
            {
                pack.SendError(looter, "This unit has no resources");
                return;
            }

            string actionName = crafterType.GatherActionName;
            string animName = crafterType.GatherAnimation;
            float gatherSeconds = crafterType.GatherSeconds;
            long level = looter.Level;
            int skillPoints = 0;

            if (looter is Character ch)
            {
                CraftingData cdata = ch.Get<CraftingData>();
                skillPoints = cdata.Get(crafterType.IdKey).GetSkillPoints(CraftingConstants.GatheringSkill);
            }

            OnStartCast startCast = new OnStartCast()
            {
                CasterId = looter.Id,
                CastSeconds = gatherSeconds,
                CastingName = actionName,
                AnimName = animName,
            };

            CompleteInteract completeInteract = new CompleteInteract()
            {
                CasterId = looter.Id,
                TargetId = target.Id,
                CrafterTypeId = crafterType.IdKey,
                Level = level,
                SkillPoints = skillPoints,
                GroundObjTypeId = 0,
                IsSkillLoot = true,
            };


            lock (looter.OnActionLock)
            {
                if (target.OnActionMessage != null && !target.OnActionMessage.IsCancelled())
                {
                    pack.SendError(looter, "Object is in use");
                    return;
                }
                else
                {
                    target.OnActionMessage = completeInteract;
                    looter.ActionMessage = completeInteract;
                }
            }

            _messageService.SendMessageNear(looter, startCast);

            _messageService.SendMessage(looter, completeInteract, gatherSeconds);

        }
    }
}
