using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Messages.RefreshGameData;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings.Services
{
    public interface IGameDataService : ISetupService
    {
        Task<GameData> LoadGameData(ServerGameState gs, bool createMissingGameData);
        Task ReloadGameData(ServerGameState gs);
        Task<bool> SaveGameData(GameData data, IRepositorySystem repoSystem);
        List<string> GetEditorIgnoreFields();
        List<IGameSettingsLoader> GetAllLoaders();
        List<IGameSettings> MapToApi(ServerGameState gs, List<IGameSettings> startSettings);
        bool SetGameDataOverrides(ServerGameState gs, IFilteredObject obj, bool forceRefresh);
        RefreshGameSettingsResult GetNewGameDataUpdates(ServerGameState gs, Character ch, bool forceRefresh);
        List<IGameSettings> GetClientGameData(ServerGameState gs, IFilteredObject obj, bool sendAllDefault);
    }
}
