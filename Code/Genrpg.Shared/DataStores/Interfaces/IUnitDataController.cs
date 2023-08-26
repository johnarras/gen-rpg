using Genrpg.Shared.DataStores.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Interfaces
{
    public interface IUnitDataContainer
    {
        IUnitData GetData();
        void SaveData(IRepositorySystem repoSystem, bool saveClean);
        void Delete(IRepositorySystem repoSystem);
    }
}
