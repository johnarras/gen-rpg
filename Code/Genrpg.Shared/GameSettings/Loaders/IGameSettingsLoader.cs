using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Loaders
{
    public interface IGameSettingsLoader
    {
        Task Setup(IRepositorySystem repoSystem);
        Type GetServerType();
        Type GetClientType();
        bool SendToClient();
        Task<List<ITopLevelSettings>> LoadAll(IRepositorySystem repoSystem, bool createDefaultIfMissing);
        ITopLevelSettings MapToApi(ITopLevelSettings settings);
    }
}
