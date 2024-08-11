using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Entities.Interfaces
{
    public interface IEntityHelper : ISetupDictionaryItem<long>
    {
        List<IIdName> GetChildList(IFilteredObject obj);

        // Find an object of the given type.
        IIdName Find(IFilteredObject obj, long id);

        string GetEditorPropertyName();
    }
}
