
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
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.AI.MessageHandlers
{
    public class AIUpdateHandler : BaseUnitServerMapMessageHandler<AIUpdate>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, AIUpdate message)
        {
            if (!IsOkUnit(unit,false))
            {
                return;
            }

            if (!_aiService.Update(rand, unit))
            {
                message.SetCancelled(true);
                return;
            }

            if (!unit.HasFlag(UnitFlags.IsDead))
            {
                if (!message.IsCancelled())
                {
                    float delayTime = _gameData.Get<AISettings>(unit).UpdateSeconds;
                    _messageService.SendMessage(unit, message, delayTime);
                }
            }
        }
    }
}
