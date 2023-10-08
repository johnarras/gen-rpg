using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Entities.Interfaces
{
    public interface IEntityHelper : ISetupDictionaryItem<long>
    {
        string GetDataPropertyName();

        // Find an object of the given type.
        IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id);
    }
}
