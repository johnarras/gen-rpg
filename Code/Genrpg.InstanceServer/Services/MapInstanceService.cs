using Genrpg.InstanceServer.Entities;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.Services
{
    public interface IMapInstanceService : ISetupService
    {
        Task AddInstanceData(AddMapInstance mapInstance);
        Task<MapInstanceData> GetInstanceDataForMap(string mapId);
        Task RemoveInstanceData(RemoveMapInstance removeInstance);

        Task AddMapServer(AddMapServer mapServer);
        Task<MapServerData> GetServerData(string mapServerId);
        Task RemoveMapServer(RemoveMapServer removeMapServer);
    }


    public class MapInstanceService : IMapInstanceService
    {

        private List<MapInstanceData> _mapInstances = new List<MapInstanceData>();

        private List<MapServerData> _mapServers = new List<MapServerData>();

        public async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }
        public async Task AddInstanceData(AddMapInstance mapInstance)
        {
            MapInstanceData instanceData = new MapInstanceData()
            {
                Host = mapInstance.Host,
                InstanceId = mapInstance.InstanceId,
                MapId = mapInstance.MapId,
                Port = mapInstance.Port,
            };

            _mapInstances.Add(instanceData);

            await Task.CompletedTask;
        }

        public async Task<MapInstanceData> GetInstanceDataForMap(string mapId)
        {
            await Task.CompletedTask;
            return _mapInstances.FirstOrDefault(x => x.MapId == mapId);
        }

        public async Task RemoveInstanceData(RemoveMapInstance removeInstance)
        {
            _mapInstances = _mapInstances.Where(x => x.InstanceId != removeInstance.FullInstanceId).ToList();
            await Task.CompletedTask;
        }

        public async Task AddMapServer(AddMapServer mapServer)
        {
            MapServerData serverData = new MapServerData()
            {
                MapServerId = mapServer.ServerId,
            };
            _mapServers.Add(serverData);

            await Task.CompletedTask;
        }

        public async Task<MapServerData> GetServerData(string mapServerId)
        {
            await Task.CompletedTask;
            return _mapServers.FirstOrDefault(x => x.MapServerId == mapServerId);
        }

        public async Task RemoveMapServer(RemoveMapServer removeMapServer)
        {
            _mapServers = _mapServers.Where(x => x.MapServerId != removeMapServer.ServerId).ToList();
            await Task.CompletedTask;
        }
    }
}
