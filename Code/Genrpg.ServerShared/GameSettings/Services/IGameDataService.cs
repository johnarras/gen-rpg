using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Login.Messages.RefreshGameSettings;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings.Services;

public interface IGameDataService : IInitializable
{
    Task<IGameData> LoadGameData(bool createMissingGameData);
    Task ReloadGameData();
    Task<bool> SaveGameData(IGameData data, IRepositoryService repoSystem);
    List<string> GetEditorIgnoreFields();
    List<IGameSettingsLoader> GetAllLoaders();
    List<ITopLevelSettings> MapToApi(List<ITopLevelSettings> startSettings);
    bool SetGameDataOverrides(IFilteredObject obj, bool forceRefresh);
    RefreshGameSettingsResult GetNewGameDataUpdates(Character ch, bool forceRefresh);
    List<ITopLevelSettings> GetClientGameData(IFilteredObject obj, bool sendAllDefault, List<ClientCachedGameSettings> clientCache = null);
}
