using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Editors.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Players.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.PlayerData
{
    public interface IUnitData : IStringId, IDirtyable, IEditorMetaDataTarget
    {
        void Save(IRepositoryService repoSystem, bool saveClean);
        List<BasePlayerData> GetSaveObjects(bool saveClean); // Must be concrete generic param for the DB to work
        void Delete(IRepositoryService repoSystem);
        void AddTo(Unit unit);
    }
}
