using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Stats.Constants;

namespace Genrpg.MapServer.Stats.MessageHandlers
{
    public class RegenHandler : BaseServerMapMessageHandler<Regen>
    {
        private IStatService _statService;
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
        }

        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, Regen message)
        {
            if (!(obj is Unit unit))
            {
                return;
            }

            float regenSeconds = StatConstants.RegenTickSeconds;

            _statService.RegenerateTick(gs, unit, regenSeconds);

            if (unit.RegenMessage != null && !unit.RegenMessage.IsCancelled())
            {
                _messageService.SendMessage(unit, message, regenSeconds);
            }
        }
    }
}
