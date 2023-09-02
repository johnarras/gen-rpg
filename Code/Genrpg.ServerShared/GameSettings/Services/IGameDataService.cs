using Genrpg.ServerShared.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Config;
using Genrpg.Shared.Interfaces;
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
        Task<DataConfig> GetDataConfig(ServerGameState gs);
        List<IGameDataLoader> GetAllLoaders();

    }

}
