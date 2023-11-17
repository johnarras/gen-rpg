using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Players.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Interfaces
{
    public interface IUnitData : IStringId, IDirtyable
    {
        void Save(IRepositorySystem repoSystem, bool saveClean);
        List<BasePlayerData> GetSaveObjects(bool saveClean);
        void Delete(IRepositorySystem repoSystem);
        void AddTo(Unit unit);
    }
}
