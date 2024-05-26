using Genrpg.MapServer.MainServer;
using Genrpg.MapServer.Maps;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Maps.Services
{
    public interface IMapServerService : IInitializable
    {
        Task Init(ServerGameState gs, InitMapServerData mapData, CancellationToken serverToken);
        IReadOnlyList<MapInstance> GetMapInstances();
        void SendAddMapServerMessage();
        Task RestartMapsWithId(string mapId);
    }
}
