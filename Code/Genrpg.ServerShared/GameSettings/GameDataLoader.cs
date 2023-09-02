using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings
{
    public class GameDataLoader<TServer, TClient> : IGameDataLoader
        where TServer : BaseGameData, new()
        where TClient : BaseGameData, new()
    {
        public virtual Type GetServerType() { return typeof(TServer); }
        public virtual string GetTypeName() { return typeof(TServer).Name; }

        public BaseGameData Create(string unitId)
        {
            TServer t = Activator.CreateInstance<TServer>();
            t.Id = unitId;
            return t;
        }

        public async Task<BaseGameData> LoadData(IRepositorySystem repoSystem, string fileName = DataConstants.DefaultFilename, bool createMissingGameData = false)
        {
            TServer data = await repoSystem.Load<TServer>(fileName);
            if (data == null && createMissingGameData)
            {
                data = new TServer() { Id = fileName };
                await repoSystem.Save(data);
            }
            return data;
        }

        public virtual BaseGameData ConvertToClientType(BaseGameData serverObject)
        {
            return SerializationUtils.ConvertType<TServer, TClient>(serverObject as TServer);
        }

        public virtual bool GoesToClient()
        {
            return true;
        }

        public async Task<List<BaseGameData>> LoadAll(IRepositorySystem repoSystem)
        {
            return (await repoSystem.Search<TServer>(x => true)).Cast<BaseGameData>().ToList();
        }

        public IGameSettingsContainer CreateContainer(BaseGameData data)
        {
            if (data is TServer tserver)
            {
                return new GameSettingsContainer<TServer>() { DataObject = tserver };
            }
            return null;
        }

        public BaseGameData Deserialize(string txt)
        {
            return SerializationUtils.Deserialize<TServer>(txt);
        }
    }
}
