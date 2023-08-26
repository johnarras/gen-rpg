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
        // Need this here or the generics don't persist through the db save process.
        void AddTo(Unit unit);
        void SaveAll(IRepositorySystem repoSystem);
    }
}
