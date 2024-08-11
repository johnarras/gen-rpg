using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Mappers
{
    public class UnitDataMapper<TServer> : IUnitDataMapper 
        where TServer : class, ITopLevelUnitData, new()
    {
        public async Task Initialize( CancellationToken token)
        {
            await Task.CompletedTask;
        }
        public virtual IUnitData MapToAPI(IUnitData serverObject)
        {
            return serverObject;
        }

        public bool SendToClient()
        {
            return !typeof(IServerOnlyData).IsAssignableFrom(typeof(TServer));
        }

        public virtual Type GetKey()
        {
            return typeof(TServer);
        }

    }
}
