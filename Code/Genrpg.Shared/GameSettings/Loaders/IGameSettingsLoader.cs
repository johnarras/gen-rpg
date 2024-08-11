using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loaders
{
    /// <summary>
    /// Use for mapping between database and server. Split from mapper so client<->server and server<->database can vary independently
    /// </summary>
    public interface IGameSettingsLoader : ISetupDictionaryItem<Type>, IInitializable
    {
        Version MinClientVersion { get; }
        Type GetChildType();
        Task<List<ITopLevelSettings>> LoadAll(IRepositoryService repoSystem, bool createDefaultIfMissing);
    }
}
