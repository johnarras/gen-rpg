using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.GameSettings.Interfaces;

namespace Genrpg.Shared.GameSettings
{
    [MessagePackObject]
    public class GameSettingsContainer<T> : IGameSettingsContainer where T : BaseGameData, new()
    {
        [Key(0)] public T DataObject { get; set; }


        public void SetObject(BaseGameData obj)
        {
            DataObject = (T)obj;
        }

        public async void Delete(IRepositorySystem repoSystem)
        {
            await repoSystem.Delete(DataObject);
        }

        public BaseGameData GetData()
        {
            return DataObject;
        }

        public async Task SaveData(IRepositorySystem repoSystem)
        {
            await repoSystem.Save(DataObject);
        }
    }
}
