using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings
{
    public interface IGameDataLoader
    {
        Type GetServerType();
        string GetTypeName();
        Task<BaseGameData> LoadData(IRepositorySystem repoSystem, string filename = DataConstants.DefaultFilename, bool createMissingGameData = false);
        BaseGameData ConvertToClientType(BaseGameData serverObject);
        bool GoesToClient();
        Task<List<BaseGameData>> LoadAll(IRepositorySystem repoSystem);
        IGameSettingsContainer CreateContainer(BaseGameData data);
        BaseGameData Deserialize(string txt);
    }
}
