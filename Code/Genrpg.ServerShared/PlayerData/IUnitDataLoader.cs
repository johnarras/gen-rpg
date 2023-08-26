using Genrpg.ServerShared.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData
{
    public interface IUnitDataLoader
    {
        Type GetServerType();
        Task<IUnitData> LoadData(IRepositorySystem repoSystem, Unit unit);
        IUnitData Create(Unit unit);
        IUnitData MapToAPI(IUnitData serverObject);
        bool ShouldSendToClient();
        void Delete(IRepositorySystem repoSystem, IUnitData data);
        Task Setup(ServerGameState gs);
    }

}
