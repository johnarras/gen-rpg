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
        Type GetServerType();
        bool SendToClient();
        Task<List<IGameSettings>> LoadAll(IRepositorySystem repoSystem, bool createDefaultIfMissing);
        IGameSettings MapToApi(IGameSettings settings);
    }
}
