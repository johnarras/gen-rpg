using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings.Services
{
    public interface IGameDataService : ISetupService
    {
        Task<GameData> LoadGameData(ServerGameState gs, bool createMissingGameData);
        Task<bool> SaveGameData(GameData data, IRepositorySystem repoSystem);
        List<string> GetEditorIgnoreFields();
        void UpdateDataBeforeSave(GameData data);
        List<IGameSettingsLoader> GetAllLoaders();
        void SetGameDataOverrides(ServerGameState gs, IFilteredObject obj, bool forceUpdate);
        List<IGameSettings> GetClientGameData(ServerGameState gs, IFilteredObject obj, bool sendAllDefault);
    }
}
