using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Mappers
{
    public interface IUnitDataMapper : ISetupDictionaryItem<Type>
    {
        public virtual Version MinClientVersion => new Version();
        IUnitData MapToAPI(IUnitData serverObject);
        bool SendToClient();
    }
}
