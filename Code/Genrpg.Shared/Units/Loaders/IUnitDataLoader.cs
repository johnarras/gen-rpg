using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Loaders
{
    public interface IUnitDataLoader
    {
        Type GetServerType();
        Task<IUnitData> LoadData(IRepositorySystem repoSystem, Unit unit);
        IUnitData Create(Unit unit);
        bool SendToClient();
        IUnitData MapToAPI(IUnitData serverObject);
        Task Setup(GameState gs);
    }

}
