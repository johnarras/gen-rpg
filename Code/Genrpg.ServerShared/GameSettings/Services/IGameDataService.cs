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

public interface IGameDataService : ISetupService
{
    Task<GameData> LoadGameData(ServerGameState gs, bool createMissingGameData);
    Task ReloadGameData(ServerGameState gs);
    Task<bool> SaveGameData(GameData data, IRepositorySystem repoSystem);
    List<string> GetEditorIgnoreFields();
    List<IGameSettingsLoader> GetAllLoaders();
    List<ITopLevelSettings> MapToApi(ServerGameState gs, List<ITopLevelSettings> startSettings);
    bool SetGameDataOverrides(ServerGameState gs, IFilteredObject obj, bool forceRefresh);
    RefreshGameSettingsResult GetNewGameDataUpdates(ServerGameState gs, Character ch, bool forceRefresh);
    List<ITopLevelSettings> GetClientGameData(ServerGameState gs, IFilteredObject obj, bool sendAllDefault, List<ClientCachedGameSettings> clientCache = null);
}
