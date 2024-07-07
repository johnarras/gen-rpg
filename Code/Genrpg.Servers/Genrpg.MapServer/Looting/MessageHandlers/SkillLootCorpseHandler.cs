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
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, SkillLootCorpse message)
        {

            if (unit.ActionMessage != null)
            {
                pack.SendError(unit, "You are already busy");
                return;
            }


            UnitType utype = _gameData.Get<UnitSettings>(unit).Get(unit.EntityId);
            if (utype == null)
            {
                pack.SendError(unit, "Not a valid target");
                return;
            }

            TribeType tribeType = _gameData.Get<TribeSettings>(unit).Get(utype.TribeTypeId);

            if (tribeType == null)
            {
                pack.SendError(unit, "Not a valid type");
                return;
            }
            CrafterType crafterType = _gameData.Get<CraftingSettings>(unit).Get(tribeType.LootCrafterTypeId);

            if (crafterType == null)
            {
                pack.SendError(unit, "This unit has no resources");
                return;
            }

            string actionName = crafterType.GatherActionName;
            string animName = crafterType.GatherAnimation;
            float gatherSeconds = crafterType.GatherSeconds;
            long level = unit.Level;
            int skillPoints = 0;

            if (unit is Character ch)
            {
                CraftingData cdata = ch.Get<CraftingData>();
                skillPoints = cdata.Get(crafterType.IdKey).GetSkillPoints(CraftingConstants.GatheringSkill);
            }

            if (unit.SkillLoot == null || unit.SkillLoot.Count < 1)
            {
                pack.SendError(unit, "Target has no loot");
                return;
            }

            OnStartCast startCast = new OnStartCast()
            {
                CasterId = unit.Id,
                CastSeconds = gatherSeconds,
                CastingName = actionName,
                AnimName = animName,
            };

            CompleteInteract completeInteract = new CompleteInteract()
            {
                CasterId = unit.Id,
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
                    pack.SendError(unit, "Object is in use");
                    return;
                }
                else
                {
                    unit.OnActionMessage = completeInteract;
                    unit.ActionMessage = completeInteract;
                }
            }

            _messageService.SendMessageNear(unit, startCast);

            _messageService.SendMessage(unit, completeInteract, gatherSeconds);

        }
    }
}
