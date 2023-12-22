using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData
{
    public interface IPlayerDataService : ISetupService
    {
        void SavePlayerData(Character ch, IRepositorySystem repoSystem, bool saveClean);
        Task<List<IUnitData>> MapToClientApi(List<IUnitData> serverDataList);
        Task<List<IUnitData>> LoadPlayerData(ServerGameState gs, Character ch);
        Task<List<CharacterStub>> LoadCharacterStubs(ServerGameState gs, string userId);
        Dictionary<Type, IUnitDataLoader> GetLoaders();
        IUnitDataLoader GetLoader<T>() where T : IUnitData;
    }
}
