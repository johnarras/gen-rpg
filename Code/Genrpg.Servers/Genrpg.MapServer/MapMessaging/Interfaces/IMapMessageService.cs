using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Genrpg.MapServer.MapMessaging.Interfaces
{
    public interface IMapMessageService : IInitializable
    {
        void Init(CancellationToken token);
        void SendMessage(MapObject mapObject, IMapMessage message, float delaySeconds = 0);

        void SendMessageNear(MapObject obj, IMapMessage message,
            float dist = MessageConstants.DefaultGridDistance,
            bool playersOnly = true,
            float delaySec = 0, List<long> filters = null);
        void UpdateGameData(IGameData gameData);
        void SendMessageToAllPlayers(IMapApiMessage message);
    }
}
