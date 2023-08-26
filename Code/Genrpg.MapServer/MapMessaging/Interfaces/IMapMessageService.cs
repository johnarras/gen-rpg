using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Genrpg.MapServer.MapMessaging.Interfaces
{
    public interface IMapMessageService : ISetupService
    {
        void Init(GameState gs, CancellationToken token);
        void SendMessage(GameState gs, MapObject mapObject, IMapMessage message, float delaySeconds = 0);

        void SendMessageNear(GameState gs, MapObject obj, IMapMessage message,
            float dist = MessageConstants.DefaultGridDistance,
            bool playersOnly = true,
            float delaySec = 0, List<long> filters = null, bool checkDistinct = false);

    }
}
