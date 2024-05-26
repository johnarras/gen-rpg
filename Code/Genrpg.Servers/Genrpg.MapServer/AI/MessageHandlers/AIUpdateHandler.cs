
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.AI.Settings;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.MapMessaging.MessageHandlers;

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
                    float delayTime = _gameData.Get<AISettings>(obj).UpdateSeconds;
                    _messageService.SendMessage(obj, message, delayTime);
                }
            }
        }
    }
}
