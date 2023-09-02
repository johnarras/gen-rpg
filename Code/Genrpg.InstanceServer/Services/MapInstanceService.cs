using Genrpg.InstanceServer.Entities;
using Genrpg.ServerShared.CloudMessaging.Servers.InstanceServer.Messaging;
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
        Task<MapInstanceData> GetInstanceData(string mapId);
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
            await Task.CompletedTask;
        }

        public async Task<MapInstanceData> GetInstanceData(string mapId)
        {
            await Task.CompletedTask;
            return null;
        }

        public async Task RemoveInstanceData(RemoveMapInstance removeInstance)
        {
            await Task.CompletedTask;
        }

        public async Task AddMapServer(AddMapServer mapServer)
        {
            await Task.CompletedTask;
        }

        public async Task<MapServerData> GetServerData(string mapServerId)
        {
            await Task.CompletedTask;
            return null;
        }

        public async Task RemoveMapServer(RemoveMapServer removeMapServer)
        {
            await Task.CompletedTask;
        }
    }
}
