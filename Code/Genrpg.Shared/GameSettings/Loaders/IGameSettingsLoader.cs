using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loaders
{
    /// <summary>
    /// Use for mapping between database and server. Split from mapper so client<->server and server<->database can vary independently
    /// </summary>
    public interface IGameSettingsLoader
    {
        Task Setup(IRepositoryService repoSystem);
        Type GetServerType();
        Task<List<ITopLevelSettings>> LoadAll(IRepositoryService repoSystem, bool createDefaultIfMissing);
    }
}
