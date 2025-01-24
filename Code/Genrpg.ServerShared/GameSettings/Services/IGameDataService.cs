using Genrpg.ServerShared.Core;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.GameSettings.WebApi.RefreshGameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings.Services;

public interface IGameDataService : IInjectable
{
    Task<IGameData> LoadGameData(bool createMissingGameData);
    Task ReloadGameData();
    Task<bool> SaveGameData(IGameData data, IRepositoryService repoSystem);
    List<string> GetEditorIgnoreFields();
    List<IGameSettingsLoader> GetAllLoaders();
    bool AcceptedByFilter(IFilteredObject obj, IPlayerFilter filter);
    List<ITopLevelSettings> MapToApi(IFilteredObject obj, List<ITopLevelSettings> startSettings);
    bool SetGameDataOverrides(IFilteredObject fobj, bool forceRefresh);
    RefreshGameSettingsResponse GetNewGameDataUpdates(IFilteredObject fobj, bool forceRefresh);
    List<ITopLevelSettings> GetClientGameData(IFilteredObject fobj, bool sendAllDefault, List<ClientCachedGameSettings> clientCache = null);
}
