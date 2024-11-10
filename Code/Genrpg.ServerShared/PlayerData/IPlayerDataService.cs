using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData
{
    public interface IPlayerDataService : IInitializable
    {
        void SavePlayerData(Character ch, bool saveAll);
        Task<List<IUnitData>> MapToClientApi(List<IUnitData> serverDataList);
        Task<List<IUnitData>> LoadAllPlayerData(IRandom rand, User user, Character ch = null);
        Task<List<CharacterStub>> LoadCharacterStubs(string userId);
        Dictionary<Type, IUnitDataLoader> GetLoaders();
        IUnitDataLoader GetLoader<T>() where T : IUnitData;
        
    }
}
