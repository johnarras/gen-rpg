using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Mappers
{
    public interface IUnitDataMapper : IInitializable
    {
        Type GetServerType();
        IUnitData MapToAPI(IUnitData serverObject);
        bool SendToClient();
    }
}
