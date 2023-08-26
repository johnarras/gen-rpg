
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.AI.Entities;

namespace Genrpg.MapServer.AI.MessageHandlers
{
    public class AIUpdateHandler : BaseServerMapMessageHandler<AIUpdate>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, AIUpdate message)
        {
            if (!GetOkUnit(obj, false, out Unit unit))
            {
                return;
            }

            if (!_aiService.Update(gs, unit))
            {
                message.SetCancelled(true);
                return;
            }

            if (!unit.HasFlag(UnitFlags.IsDead))
            {
                if (!message.IsCancelled())
                {
                    float delayTime = gs.data.GetGameData<AISettings>().UpdateSeconds;
                    _messageService.SendMessage(gs, obj, message, delayTime);
                }
            }
        }
    }
}
