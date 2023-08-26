using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameDatas.Interfaces
{
    public interface IGameDataContainer
    {
        void SetObject(BaseGameData obj);
        BaseGameData GetData();
        Task SaveData(IRepositorySystem repoSystem);
        void Delete(IRepositorySystem repoSystem);
    }
}
