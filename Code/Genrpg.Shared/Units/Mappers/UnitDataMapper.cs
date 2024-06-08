using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Mappers
{
    public class UnitDataMapper<TServer> : IUnitDataMapper 
        where TServer : class, ITopLevelUnitData, new()
    {
        public virtual IUnitData MapToAPI(IUnitData serverObject)
        {
            return serverObject;
        }

        public virtual bool SendToClient()
        {
            return true;
        }

        public virtual Type GetServerType()
        {
            return typeof(TServer);
        }

        public virtual async Task Initialize(IGameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}
