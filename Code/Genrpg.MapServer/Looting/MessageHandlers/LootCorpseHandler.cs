using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Loot.Messages;

namespace Genrpg.MapServer.Looting.MessageHandlers
{
    public class LootCorpseHandler : BaseServerMapMessageHandler<LootCorpse>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, LootCorpse message)
        {
            if (!_objectManager.GetUnit(message.UnitId, out Unit unit))
            {
                pack.SendError(gs, obj, "That can't be looted");
                return;
            }

            if (!UnitUtils.AttackerInfoMatchesObject(unit.GetFirstAttacker(),obj))
            {
                pack.SendError(gs, obj, "You can't loot that!");
                return;
            }


            List<SpawnResult> loot = new List<SpawnResult>();
            lock (unit.OnActionLock)
            {
                if (unit.Loot == null || unit.Loot.Count < 1)
                {
                    pack.SendError(gs, obj, "That has no loot");
                    return;
                }
                loot = unit.Loot;
                unit.Loot = null;
            }

            if (obj is Character ch)
            {
                _entityService.GiveRewards(gs, ch, loot);
                SendRewards sendLoot = new SendRewards()
                {
                    ShowPopup = true,
                    Rewards = loot,
                };
                ch.AddMessage(sendLoot);
            }

            _messageService.SendMessageNear(unit, new ClearLoot() { UnitId = unit.Id });
        }
    }
}
