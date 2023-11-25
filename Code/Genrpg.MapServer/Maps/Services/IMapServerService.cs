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

namespace Genrpg.MapServer.Services.Maps
{
    public interface IMapServerService : IService
    {
        Task Init(ServerGameState gs, InitMapServerData mapData, CancellationToken serverToken);
        List<MapInstance> GetMapInstances();
        public void SendAddMapServerMessage();
    }
}
