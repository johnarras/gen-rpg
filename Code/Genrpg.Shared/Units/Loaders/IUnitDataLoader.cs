using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Loaders
{
    public interface IUnitDataLoader : IInitializable
    {
        Type GetServerType();
        Task<ITopLevelUnitData> LoadFullData(Unit unit);
        Task<ITopLevelUnitData> LoadTopLevelData(Unit unit);
        Task<IChildUnitData> LoadChildByIdkey(Unit unit, long childIdkey);
        Task<IChildUnitData> LoadChildById(Unit unit, string childId);

        IUnitData Create(Unit unit);
    }

}
